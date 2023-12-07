using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;


namespace Lab3_OOP
{
    internal class JsonProcessor
    {
        //метод що відповідає за серіалізацію даних в джсон, тобто перетворення в формат джсон
        public static void Serialize(string path, ObservableCollection<ScientificWork> results)
        {
            //створюємо налаштування серіалізації
            var options = new JsonSerializerOptions
            {
                //красиве перенесення рядків в джсон
                WriteIndented = true,
                //кодування даних довільне встановлюємо
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            using (FileStream fstream = new FileStream(path, FileMode.Create))
            {
                //викликає бібліотечний метод серіалізації
                JsonSerializer.Serialize(fstream, results, options);
            }
        }
        //метод що відповідає за десеріалізацію даних в джсон, тобто перетворення з формату джсон
        public static ObservableCollection<ScientificWork> Deserialize(string path)
        {
            ObservableCollection<ScientificWork> results = new ObservableCollection<ScientificWork>();
            using (FileStream fstream = new FileStream(path, FileMode.Open))
            {
                //викликаємо бібілотечний метод десерілаізації даних
                var works = JsonSerializer.Deserialize<List<ScientificWork>>(fstream);

                //записуємо дані в результуючу колекцію
                foreach (ScientificWork work in works)
                {
                    results.Add(work);
                }

                return results;
            }
        }
    }
}
