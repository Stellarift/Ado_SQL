namespace CarRentalDbFirst
{
    public class Car
    {
        public int Id { get; set; }
        public string Model { get; set; } = "";
        public int ReleaseYear { get; set; }
        public decimal PricePerDay { get; set; }
        public int BrandId { get; set; }
        public Brand? Brand { get; set; } // Навигационное свойство
    }
}