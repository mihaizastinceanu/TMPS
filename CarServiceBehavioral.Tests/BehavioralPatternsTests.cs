using CarServiceBehavioral.Domain.Builder;
using CarServiceBehavioral.Domain.Models;
using CarServiceBehavioral.Domain.Models.Parts;
using CarServiceBehavioral.Domain.Models.Services;
using CarServiceBehavioral.Domain.Observer;
using CarServiceBehavioral.Domain.Strategy;

namespace CarServiceBehavioral.Tests;

public class BehavioralPatternsTests
{
    private static readonly Vehicle Logan = new("Ion Popescu", "Dacia", "Logan", "C AB 123", 2018);

    // Oil change with aftermarket oil: 300 labor + 430 oil = 730 MDL
    private static ServiceOrder NewOrder(bool urgent = false)
    {
        var builder = new ServiceOrderBuilder().ForVehicle(Logan).AddService(new OilChange(new AftermarketEngineOil()));
        if (urgent)
            builder.MarkUrgent();
        return builder.Build();
    }

    private static ServiceOrder ReadyOrder(bool urgent = false)
    {
        var order = NewOrder(urgent);
        order.StartWork();
        order.FinishWork();
        return order;
    }

    // ---------- State ----------

    [Fact]
    public void State_NewOrderIsReceived()
    {
        Assert.Equal(OrderStatus.Received, NewOrder().Status);
    }

    [Fact]
    public void State_FollowsTheWholeLifecycle()
    {
        var order = NewOrder();

        order.StartWork();
        Assert.Equal(OrderStatus.InProgress, order.Status);
        order.WaitForParts("oil filter");
        Assert.Equal(OrderStatus.WaitingForParts, order.Status);
        order.PartsArrived();
        Assert.Equal(OrderStatus.InProgress, order.Status);
        order.FinishWork();
        Assert.Equal(OrderStatus.ReadyForPickup, order.Status);
        new Cashier(new RegularPricing()).Checkout(order);
        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Fact]
    public void State_CannotFinishBeforeStarting()
    {
        var order = NewOrder();

        Assert.Throws<InvalidOperationException>(order.FinishWork);
        Assert.Equal(OrderStatus.Received, order.Status);
    }

    [Fact]
    public void State_CannotDeliverWhileWaitingForParts()
    {
        var order = NewOrder();
        order.StartWork();
        order.WaitForParts("brake pads");

        Assert.Throws<InvalidOperationException>(() => new Cashier(new RegularPricing()).Checkout(order));
        Assert.Null(order.Invoice);
    }

    [Fact]
    public void State_CancelledOrderAcceptsNoActions()
    {
        var order = NewOrder();
        order.Cancel("customer changed mind");

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Throws<InvalidOperationException>(order.StartWork);
        Assert.Throws<InvalidOperationException>(() => order.Cancel("again"));
    }

    [Fact]
    public void State_FinishedOrderCannotBeCancelled()
    {
        var order = ReadyOrder();

        Assert.Throws<InvalidOperationException>(() => order.Cancel("too late"));
        Assert.Equal(OrderStatus.ReadyForPickup, order.Status);
    }

    [Fact]
    public void State_RejectsInvoiceOfAnotherOrder()
    {
        var order = ReadyOrder();
        var wrongInvoice = new Invoice(order.Number + 1000, "Regular", 730m, 0m);

        Assert.Throws<ArgumentException>(() => order.Deliver(wrongInvoice));
        Assert.Equal(OrderStatus.ReadyForPickup, order.Status);
    }

    // ---------- Observer ----------

    [Fact]
    public void Observer_DashboardReceivesEveryChange()
    {
        var dashboard = new WorkshopDashboard(_ => { });
        var order = NewOrder();
        order.Attach(dashboard);

        order.StartWork();
        order.WaitForParts("oil filter");
        order.PartsArrived();
        order.FinishWork();

        Assert.Equal(4, dashboard.History.Count);
        Assert.Equal(OrderStatus.Received, dashboard.History[0].From);
        Assert.Equal(OrderStatus.WaitingForParts, dashboard.History[1].To);
        Assert.Equal("oil filter", dashboard.History[1].Note);
        Assert.Equal(1, dashboard.CountIn(OrderStatus.ReadyForPickup));
    }

    [Fact]
    public void Observer_CustomerGetsSmsOnlyForImportantChanges()
    {
        var messages = new List<string>();
        var order = NewOrder();
        order.Attach(new CustomerSmsNotifier(messages.Add));

        order.StartWork();            // no SMS
        order.FinishWork();           // SMS: ready

        Assert.Single(messages);
        Assert.Contains("Ion Popescu", messages[0]);
        Assert.Contains("ready", messages[0]);
    }

    [Fact]
    public void Observer_DetachedObserverIsNotNotified()
    {
        var dashboard = new WorkshopDashboard(_ => { });
        var order = NewOrder();
        order.Attach(dashboard);
        order.Attach(dashboard);      // attached twice -> still notified once

        order.StartWork();
        order.Detach(dashboard);
        order.FinishWork();

        Assert.Single(dashboard.History);
    }

    [Fact]
    public void Observer_AccountingRecordsOnlyDeliveredOrders()
    {
        var accounting = new AccountingDepartment();
        var delivered = ReadyOrder();
        var cancelled = NewOrder();
        delivered.Attach(accounting);
        cancelled.Attach(accounting);

        cancelled.Cancel("no time");
        new Cashier(new RegularPricing()).Checkout(delivered);

        Assert.Single(accounting.Invoices);
        Assert.Equal(730m, accounting.Revenue);
    }

    // ---------- Strategy ----------

    [Fact]
    public void Strategy_RegularPricingGivesNoDiscount()
    {
        var invoice = new Cashier(new RegularPricing()).Checkout(ReadyOrder());

        Assert.Equal(730m, invoice.Subtotal);
        Assert.Equal(0m, invoice.Discount);
        Assert.Equal(730m, invoice.Total);
    }

    [Fact]
    public void Strategy_LoyaltyCardTakesTenPercentOff()
    {
        var invoice = new Cashier(new LoyaltyCardPricing()).Checkout(ReadyOrder());

        Assert.Equal(73m, invoice.Discount);
        Assert.Equal(657m, invoice.Total);
    }

    [Fact]
    public void Strategy_FleetContractRemovesUrgentFee()
    {
        // urgent: 730 + 20% = 876; fleet pays 730 - 5% = 693.50
        var invoice = new Cashier(new FleetContractPricing()).Checkout(ReadyOrder(urgent: true));

        Assert.Equal(876m, invoice.Subtotal);
        Assert.Equal(693.50m, invoice.Total);
    }

    [Fact]
    public void Strategy_CanBeChangedAtRuntime()
    {
        var cashier = new Cashier(new RegularPricing());
        var first = cashier.Checkout(ReadyOrder());

        cashier.SetStrategy(new LoyaltyCardPricing(20m));
        var second = cashier.Checkout(ReadyOrder());

        Assert.Equal(730m, first.Total);
        Assert.Equal(584m, second.Total);
        Assert.StartsWith("Loyalty card", second.PricingName);
    }

    [Fact]
    public void Strategy_LoyaltyCardRejectsInvalidPercent()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LoyaltyCardPricing(150m));
    }
}
