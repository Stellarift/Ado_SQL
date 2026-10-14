using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AdoNetLab2
{
    class Program
    {
        static string cs = "";

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            cs = config.GetConnectionString("MyConnString")!;

            Task1_ManualDataTable();
            Task2_FillDataSet();
            Task3_AutoUpdate();
            Task4_CustomUpdate();
            Task5_RowStateOrder();
            Task6_TableMappings();

            Console.WriteLine("\nГотово. Нажмите любую клавишу...");
            Console.ReadKey();
        }

        // Вспомогательный метод вывода
        static void PrintTable(DataTable table, string title = "")
        {
            if (!string.IsNullOrEmpty(title))
                Console.WriteLine($"\n {title} ");

            foreach (DataColumn col in table.Columns)
                Console.Write(col.ColumnName.PadRight(15));
            Console.WriteLine();
            Console.WriteLine(new string('-', table.Columns.Count * 15));

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    object val = row[col];
                    string s = val == DBNull.Value ? "NULL" : val.ToString()!;
                    Console.Write(s.PadRight(15));
                }
                Console.WriteLine();
            }
        }

        // 1: DataTable вручную
        static void Task1_ManualDataTable()
        {
            Console.WriteLine("\n1) DataTable вручную");

            DataTable table = new DataTable("Authors");
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("FirstName", typeof(string));
            table.Columns.Add("LastName", typeof(string));

            table.Rows.Add(1, "Isaac",    "Asimov");
            table.Rows.Add(2, "Ray",      "Bradbury");
            table.Rows.Add(3, "Clifford", "Simak");
            table.Rows.Add(4, "Roger",    "Zelazny");

            PrintTable(table, "Authors (manual)");

            // Демонстрация чтения по индексу и по имени
            Console.WriteLine($"\nrow[0][0] = {table.Rows[0][0]}");
            Console.WriteLine($"row[0][\"FirstName\"] = {table.Rows[0]["FirstName"]}");
        }

        // 2) DataSet + SqlDataAdapter
        static void Task2_FillDataSet()
        {
            Console.WriteLine("\n2) DataSet + SqlDataAdapter");

            using SqlConnection conn = new SqlConnection(cs);
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Books", conn);
            DataSet ds = new DataSet();
            da.Fill(ds, "Books");

            PrintTable(ds.Tables["Books"]!, "Books (из БД)");

            // Ответ на контрольный вопрос
            Console.WriteLine("\nКонтрольный вопрос: если Fill() вызвать дважды с одним именем —");
            Console.WriteLine("данные НЕ задублируются, таблица просто обновится.");
        }

        //3: SqlCommandBuilder
        static void Task3_AutoUpdate()
        {
            Console.WriteLine("\n3) SqlCommandBuilder");

            using SqlConnection conn = new SqlConnection(cs);
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Books", conn);
            SqlCommandBuilder cmdBuilder = new SqlCommandBuilder(da);

            DataSet ds = new DataSet();
            da.Fill(ds, "Books");

            Console.WriteLine("До изменения:");
            PrintTable(ds.Tables["Books"]!, "Books (до)");

            // Меняем цену первой книги
            ds.Tables["Books"]!.Rows[0]["Price"] = 999;

            Console.WriteLine("\nПосле локального изменения:");
            PrintTable(ds.Tables["Books"]!, "Books (после локального изменения)");

            // Синхронизация с БД
            int rows = da.Update(ds, "Books");
            Console.WriteLine($"\nОбновлено строк в БД: {rows}");

            // Перечитываем из БД
            DataSet ds2 = new DataSet();
            da.Fill(ds2, "Books");
            PrintTable(ds2.Tables["Books"]!, "Books (перечитано из БД)");

            // Показываем сгенерированные команды
            Console.WriteLine("\nСгенерированные команды:");
            Console.WriteLine("INSERT: " + cmdBuilder.GetInsertCommand().CommandText);
            Console.WriteLine("UPDATE: " + cmdBuilder.GetUpdateCommand().CommandText);
            Console.WriteLine("DELETE: " + cmdBuilder.GetDeleteCommand().CommandText);

            Console.WriteLine("\nПроблемный эксперимент: если в SELECT нет первичного ключа,");
            Console.WriteLine("SqlCommandBuilder не сможет сгенерировать UPDATE/DELETE,");
            Console.WriteLine("и выбросит InvalidOperationException.");
        }

        //4) пользовательский UpdateCommand
        static void Task4_CustomUpdate()
        {
            Console.WriteLine("\n4) пользовательский UpdateCommand");

            using SqlConnection conn = new SqlConnection(cs);
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Books", conn);

            SqlCommand updateCmd = new SqlCommand(
                "UPDATE Books SET Price = @pPrice WHERE Id = @pId", conn);

            updateCmd.Parameters.Add(new SqlParameter("@pPrice", SqlDbType.Int));
            updateCmd.Parameters["@pPrice"].SourceVersion = DataRowVersion.Current;
            updateCmd.Parameters["@pPrice"].SourceColumn = "Price";

            updateCmd.Parameters.Add(new SqlParameter("@pId", SqlDbType.Int));
            updateCmd.Parameters["@pId"].SourceVersion = DataRowVersion.Original;
            updateCmd.Parameters["@pId"].SourceColumn = "Id";

            da.UpdateCommand = updateCmd;

            DataSet ds = new DataSet();
            da.Fill(ds, "Books");

            // Меняем цену двух книг
            ds.Tables["Books"]!.Rows[0]["Price"] = 111;
            ds.Tables["Books"]!.Rows[1]["Price"] = 222;

            int rows = da.Update(ds, "Books");
            Console.WriteLine($"Обновлено строк: {rows}");

            DataSet ds2 = new DataSet();
            da.Fill(ds2, "Books");
            PrintTable(ds2.Tables["Books"]!, "Books (после UpdateCommand)");
        }

        //5) DataViewRowState — порядок синхронизации
        static void Task5_RowStateOrder()
{
    Console.WriteLine("\n--- Задание 5: DataViewRowState ---");

    using SqlConnection conn = new SqlConnection(cs);
    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Books", conn);
    SqlCommandBuilder cmdBuilder = new SqlCommandBuilder(da);

    DataSet ds = new DataSet();
    da.Fill(ds, "Books");
    DataTable books = ds.Tables["Books"]!;

    // Удаляем первую строку
    books.Rows[0].Delete();

    // Меняем цену второй
    books.Rows[1]["Price"] = 777;

    // новую строку
    DataRow newRow = books.NewRow();
    newRow["Title"]  = "New Book";
    newRow["Author"] = "New Author";
    newRow["Price"]  = 500;
    newRow["Pages"]  = 100;
    books.Rows.Add(newRow);

    Console.WriteLine("\nСостояния строк:");
    foreach (DataRow row in books.Rows)
    {
        string title;
        if (row.RowState == DataRowState.Deleted)
            title = row["Title", DataRowVersion.Original]?.ToString() ?? "";
        else
            title = row["Title"]?.ToString() ?? "";

        Console.WriteLine($"  {title,25} -> {row.RowState}");
    }

    // Deleted -> Modified -> Added
    da.Update(books.Select(null, null, DataViewRowState.Deleted));
    da.Update(books.Select(null, null, DataViewRowState.ModifiedCurrent));
    da.Update(books.Select(null, null, DataViewRowState.Added));

    DataSet ds2 = new DataSet();
    da.Fill(ds2, "Books");
    PrintTable(ds2.Tables["Books"]!, "Books (после Update в правильном порядке)");
}

        //6) TableMappings
        static void Task6_TableMappings()
        {
            Console.WriteLine("\n6) TableMappings");

            using SqlConnection conn = new SqlConnection(cs);

            // Без TableMappings
            SqlDataAdapter da1 = new SqlDataAdapter(
                "SELECT * FROM Authors; SELECT * FROM Books;", conn);
            DataSet ds1 = new DataSet();
            da1.Fill(ds1);

            Console.WriteLine("Без TableMappings — имена таблиц по умолчанию:");
            foreach (DataTable t in ds1.Tables)
                Console.WriteLine($"  {t.TableName}");

            // С TableMappings
            SqlDataAdapter da2 = new SqlDataAdapter(
                "SELECT * FROM Authors; SELECT * FROM Books;", conn);
            da2.TableMappings.Add("Table",  "Authors");
            da2.TableMappings.Add("Table1", "Books");

            DataSet ds2 = new DataSet();
            da2.Fill(ds2);

            Console.WriteLine("\nС TableMappings — переименованные:");
            foreach (DataTable t in ds2.Tables)
                Console.WriteLine($"  {t.TableName}");

            PrintTable(ds2.Tables["Authors"]!, "Authors (по имени)");
            PrintTable(ds2.Tables["Books"]!,   "Books (по имени)");
        }
    }
}
