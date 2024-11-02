namespace module7


{
    internal class Program
    {
        static void Main()
        {

            var package = new SolidProduct("some crap") * 4;
            package += new FragileProduct("fragile crap");
            foreach (var item in package) { item.DisplayName(); }
            var dd = new DroneDelivery();
            var order = new Order<Delivery, Product>(dd, package, 124212);
            order.Delivery.DeliverAnyways(order);
            Console.ReadKey();

        }

    }    
    
}
