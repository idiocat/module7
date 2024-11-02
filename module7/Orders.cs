namespace module7
{
    public class RNG
    {
        private static protected Random random = new Random();
        public static int rng(int a, int b) => random.Next(a, b);
        public static int rng(int a) => random.Next(a);
    }

    abstract class Delivery
    {
        public string Address { get; set; }
        public int Days { get; set; }
        private string phone;
        public string Phone
        {
            get { return phone; }
            set
            {
                if (!value.StartsWith("89")) { throw new DivideByZeroException(); }
                else if (value.Length != 11) { throw new ArithmeticException(); }
                else { phone = value; }
            }
        }

        public Delivery(string address = null, int days = -1, string phone = null)
        {
            if (address == null) { }
            else { Address = address; }

            if (days == -1) { Days = RNG.rng(2, 22); }
            else { Days = days; }

            if (phone == null)
            {
                string phoneRNG = "89";
                for (int i = 0; i < 9; ++i) { phoneRNG += RNG.rng(9).ToString(); }
                Phone = phoneRNG;
            }
            else { Phone = phone; }
        }

        internal virtual void Deliver(Order<Delivery, Product> order) { Console.Write("Delivery method is invalid."); }
    }

    class HomeDelivery : Delivery
    {
        internal override void Deliver(Order<Delivery, Product> order)
        {
            Console.Write($"Delivering the order {order.Number} to {Address} in {Days * order.Package.DeliverySpeedMultiplier} days.");
        }
    }

    class PickPointDelivery : Delivery
    {
        internal override void Deliver(Order<Delivery, Product> order)
        {
            if (order.Package.Has(typeof(PerishableProduct))) { Console.WriteLine($"The order {order.Number} can't be delivered."); }
            else { Console.Write($"Delivering the order {order.Number} to {Address} in {Days * order.Package.DeliverySpeedMultiplier} days."); }
        }
    }

    class ShopDelivery : Delivery
    {
        internal override void Deliver(Order<Delivery, Product> order)
        {
            Console.Write($"Delivering the order {order.Number} to {Address} in {Days * order.Package.DeliverySpeedMultiplier} days.");
        }
    }
    class DroneDelivery : Delivery
    {
        internal override void Deliver(Order<Delivery, Product> order)
        {
            foreach (Product product in order.Package)
            {
                if (product.canElectrocuteDrone)
                {
                    Console.WriteLine($"The order {order.Number} can't be delivered.");
                    return;
                }
            }
            Console.Write($"Delivering the order {order.Number} to {Address} in {Days * order.Package.DeliverySpeedMultiplier} days.");
        }
    }

    class Order<TDelivery, TProduct>
        where TDelivery : Delivery
        where TProduct : Product
    {
        public TDelivery Delivery;
        public Package<TProduct> Package;
        public int Number;

        public Order(TDelivery delivery, Package<TProduct> package, int number)
        {
            Delivery = delivery;
            Package = package;
            Number = number;
        }

        public void DisplayAddress()
        {
            Console.WriteLine(Delivery.Address);
        }
        public void DisplayProducts()
        {
            foreach (var product in Package)
            {
                product.DisplayName();
            }
        }
    }

    abstract class Product
    {
        public string Name;
        public string? Description;
        public abstract double DeliverySpeedMultiplier { get; set; }
        public abstract int TimeLimitInDays { get; set; }
        public abstract bool canElectrocuteDrone { get; set; }

        public Product(string name, string description = null)
        {
            Name = name;
            Description = description;
        }

        public void DisplayName() { Console.WriteLine(Name); }

        public static Package<Product> operator +(Product a, Product b) => new Package<Product>(a, b);
        public static Package<Product> operator *(Product product, int mult)
        {
            Product[] package = new Product[mult];
            Array.Fill(package, product);
            return new Package<Product>(package);
        }

    }
    //product type may affect delivery time and/or conditions
    class SolidProduct : Product
    {
        public override double DeliverySpeedMultiplier { get; set; } = 1;
        public override int TimeLimitInDays { get; set; } = -1;
        public override bool canElectrocuteDrone { get; set; } = false;
        public SolidProduct(string name, string description = null) : base(name, description) { }
    }
    class FragileProduct : Product
    {
        public override double DeliverySpeedMultiplier { get; set; } = .5;
        public override int TimeLimitInDays { get; set; } = -1;
        public override bool canElectrocuteDrone { get; set; } = false;
        public FragileProduct(string name, string description = null) : base(name, description) { }
    }
    class LiquidProduct : Product
    {
        public override double DeliverySpeedMultiplier { get; set; } = 1;
        public override int TimeLimitInDays { get; set; } = -1;
        public override bool canElectrocuteDrone { get; set; } = false;
        public LiquidProduct(string name, string description = null) : base(name, description) { }
    }
    class PerishableProduct : Product
    {
        public override double DeliverySpeedMultiplier { get; set; } = 1;
        public override int TimeLimitInDays { get; set; } = 2;
        public override bool canElectrocuteDrone { get; set; } = false;
        public PerishableProduct(string name, string description = null) : base(name, description) { }
    }
    //products must be packed before being delivered
    struct Package<TProduct>
        where TProduct : Product
    {
        public int Size = 0;
        public PackageInstance<TProduct>? top = null;
        
        public Package(params TProduct[] products)
        {
            foreach (TProduct product in products) { AddProduct(product); }
        }
        public double DeliverySpeedMultiplier = 1;
        private void AddProduct(TProduct product)
        {
            if (top is null) { top = new PackageInstance<TProduct>(product); }
            else { top = new PackageInstance<TProduct>(product, top); }
            if (top.product.DeliverySpeedMultiplier < DeliverySpeedMultiplier) { DeliverySpeedMultiplier = top.product.DeliverySpeedMultiplier; }
            ++Size;
        }
        
        public bool Has(Type productType)
        {
            foreach (TProduct product in this)
            {
                if (product.GetType() == productType) { return true; }
            }
            return false;
        } 

        public IEnumerator<Product> GetEnumerator()
        {
            PackageInstance<TProduct>? top = this.top;
            while (true)
            {
                if (!(top is null)) { yield return top.product; }
                try { top = top.next; }
                catch (NullReferenceException) { break; }
            }
        }

        public static Package<TProduct> operator +(Package<TProduct> package, TProduct product)
        {
            package.AddProduct(product);
            return package;
        }
        public static Package<TProduct> operator +(Package<TProduct> a, Package<TProduct> b)
        {
            foreach (TProduct product in b) { a.AddProduct(product); }
            return a;
        }

    }
    class PackageInstance<TProduct>
        where TProduct : Product
    {
        public Product product;
        public PackageInstance<TProduct>? next;
        public PackageInstance(TProduct product, PackageInstance<TProduct> next = null)
        {
            this.product = product;
            this.next = next;
        }
    }
}

