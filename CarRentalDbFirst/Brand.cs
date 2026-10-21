using System.Collections.Generic;

namespace CarRentalDbFirst
{
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Country { get; set; } = "";
        public List<Car> Cars { get; set; } = new(); // Навигационное свойство
    }
}