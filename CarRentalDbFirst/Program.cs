using System;
using System.Linq;

namespace CarRentalDbFirst
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Лабораторная работа: АВТОПАРК \n");

            Task1_AddInitialData();
            Task2_GetAllCars();
            Task3_FindCarByModel("Camry");
            Task4_GetCarById(1);
            Task5_GetCarsWithBrandInfo();
            Task6_AddNewCar("X5", 2023, 150.00m, 1);
            Task7_UpdateCarPrice(1, 200.00m);

            Console.ReadLine();
        }

        // задание 0. Заполнение базы данных
        static void Task1_AddInitialData()
        {
            Console.WriteLine("Добавление начальных данных...");
            using (CarRentalContext db = new CarRentalContext())
            {
                if (!db.Brands.Any())
                {
                    var toyota = new Brand { Name = "Toyota", Country = "Япония" };
                    var bmw = new Brand { Name = "BMW", Country = "Германия" };
                    db.Brands.Add(toyota);
                    db.Brands.Add(bmw);
                    db.SaveChanges(); // чтобы получить Id

                    db.Cars.Add(new Car { Model = "Camry", ReleaseYear = 2020, PricePerDay = 50.00m, BrandId = toyota.Id });
                    db.Cars.Add(new Car { Model = "Corolla", ReleaseYear = 2019, PricePerDay = 40.00m, BrandId = toyota.Id });
                    db.Cars.Add(new Car { Model = "X5", ReleaseYear = 2022, PricePerDay = 100.00m, BrandId = bmw.Id });
                    db.SaveChanges();
                    Console.WriteLine("Данные успешно добавлены!");
                }
                else
                {
                    Console.WriteLine("База данных уже содержит записи.");
                }
            }
        }

        // Задание 1. Получение списка (ToList, OrderBy)
        static void Task2_GetAllCars()
        {
            Console.WriteLine("\nЗАДАНИЕ 1: Все автомобили по алфавиту");
            using (CarRentalContext db = new CarRentalContext())
            {
                var cars = db.Cars.OrderBy(c => c.Model).ToList();
                foreach (var car in cars)
                {
                    Console.WriteLine($"{car.Model} — {car.PricePerDay:C}");
                }
            }
        }

        // Задание 2. Поиск с условием (Where, FirstOrDefault)
        static void Task3_FindCarByModel(string modelName)
        {
            Console.WriteLine($"\nЗАДАНИЕ 2: Поиск машины {modelName}");
            using (CarRentalContext db = new CarRentalContext())
            {
                var car = db.Cars.FirstOrDefault(c => c.Model == modelName);
                if (car != null)
                    Console.WriteLine($"Найдена: {car.Model}, Год: {car.ReleaseYear}");
                else
                    Console.WriteLine("Машина не найдена");
            }
        }

        // Задание 3. Поиск по ID (Find)
        static void Task4_GetCarById(int id)
        {
            Console.WriteLine($"\nЗАДАНИЕ 3: Поиск по ID ({id})");
            using (CarRentalContext db = new CarRentalContext())
            {
                var car = db.Cars.Find(id);
                if (car != null)
                    Console.WriteLine($"Модель: {car.Model}");
                else
                    Console.WriteLine("Машина не найдена");
            }
        }

        // Задание 4. Свойства навигации
        static void Task5_GetCarsWithBrandInfo()
        {
            Console.WriteLine("\nЗАДАНИЕ 4: Автомобили с названием марки");
            using (CarRentalContext db = new CarRentalContext())
            {
                var cars = db.Cars.ToList();
                foreach (var car in cars)
                {
                    Console.WriteLine($"Модель: {car.Model} | Марка: {car.Brand?.Name} | Страна: {car.Brand?.Country}");
                }
            }
        }

        // Задание 5. Добавление данных (Add, SaveChanges)
        static void Task6_AddNewCar(string model, int year, decimal price, int brandId)
        {
            Console.WriteLine($"\nЗАДАНИЕ 5: Добавление машины '{model}'");
            using (CarRentalContext db = new CarRentalContext())
            {
                var newCar = new Car { Model = model, ReleaseYear = year, PricePerDay = price, BrandId = brandId };
                db.Cars.Add(newCar);
                db.SaveChanges();
                Console.WriteLine("Машина успешно добавлена!");
            }
        }

        // Задание 6. Изменение данных (Update, SaveChanges)
        static void Task7_UpdateCarPrice(int cardId, decimal newPrice)
        {
            Console.WriteLine($"\nЗАДАНИЕ 6: Обновление цены машины (ID: {cardId})");
            using (CarRentalContext db = new CarRentalContext())
            {
                var car = db.Cars.Find(cardId);
                if (car != null)
                {
                    car.PricePerDay = newPrice;
                    db.SaveChanges();
                    Console.WriteLine("Цена успешно обновлена!");
                }
                else
                {
                    Console.WriteLine("Машина не найдена");
                }
            }
        }
    }
}
