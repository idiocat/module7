namespace module7
{
    static class Extensions
    {
        public static void DeliverAnyways(this Delivery source, Order<Delivery, Product> order)
        {
            Console.Write($"Delivering the order {order.Number} to {source.Address} in {source.Days * order.Package.DeliverySpeedMultiplier} days in any conditions." +
                $"\nPlease leave no complaints.");
        }
    }
}
