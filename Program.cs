using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.ObjectModel;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace SeleniumLoginExample
{
    public class CarModel
    {
        public string Model { get; set; }
        public List<string> Generations { get; set; }
    }

    public class CarMake
    {
        public string Make { get; set; }
        public List<CarModel> Models { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Получение списка уже записанных марок
            List<string> completedMakes = new List<string>();
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "manufacturer_*.json");
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var makeName = fileName.Replace("manufacturer_", "");
                completedMakes.Add(makeName);
            }

            // Создание экземпляра ChromeDriver
            IWebDriver driver = new ChromeDriver();

            try
            {
                // Загрузка страницы
                driver.Navigate().GoToUrl("https://auto.ru/garage/add/dreamcar/");

                Console.Write("Авторизируйся и нажми Enter\n");
                Console.ReadLine();

                driver.Navigate().GoToUrl("https://auto.ru/garage/add/dreamcar/");
                Thread.Sleep(2000);

                // Нажатие на блок с указанным классом
                IWebElement openAll = driver.FindElement(By.CssSelector(".Link.FormSection__ListItem.MarkFormSection__link"));
                openAll.Click();
                Thread.Sleep(1000);

                ReadOnlyCollection<IWebElement> marks = driver.FindElements(By.CssSelector(".FormSection__ListItem"));
                Thread.Sleep(500);

                bool startParsing = completedMakes.Count == 0;

                // Выполнение действий с каждым элементом
                foreach (var mark in marks)
                {
                    Thread.Sleep(100);

                    IWebElement markElement = mark;
                    string makeName = markElement.Text;
                    if (makeName == "Batmobile" || makeName == "Vauxhall" || makeName == "Атом")
                    {
                        continue;
                    }

                    // Если текущая марка уже записана, пропускаем ее
                    if (!startParsing && completedMakes.Contains(makeName))
                    {
                        continue;
                    }

                    startParsing = true;

                    Thread.Sleep(500);
                    markElement.Click();
                    Thread.Sleep(500);

                    // Нахождение всех моделей
                    ReadOnlyCollection<IWebElement> radioList = driver.FindElements(By.CssSelector(".RadioList"));
                    Thread.Sleep(500);

                    try
                    {
                        IWebElement allModel = driver.FindElement(By.CssSelector(".Link.FormSection__ListItem.ModelFormSection__link"));
                        allModel.Click();
                    }
                    catch (NoSuchElementException)
                    {

                    }

                    ReadOnlyCollection<IWebElement> models = radioList[0].FindElements(By.CssSelector(".Radio__text"));
                    Thread.Sleep(500);

                    List<CarModel> carModels = new List<CarModel>();

                    // Перебор моделей
                    foreach (var model in models)
                    {
                        IWebElement modelElement = model;
                        Thread.Sleep(300);
                        string modelName = modelElement.Text;
                        if (modelName == "Transit" || modelName == "Aura" || modelName == "Egoista" || modelName == "Exelero")
                        {
                            continue;
                        }
                        modelElement.Click();


                        Thread.Sleep(500);
                        ReadOnlyCollection<IWebElement> radioList_2 = driver.FindElements(By.CssSelector(".RadioList"));
                        /*Thread.Sleep(200);*/
                        ReadOnlyCollection<IWebElement> generations = radioList_2[1].FindElements(By.CssSelector(".Radio__text"));
                        Thread.Sleep(300);

                        if (generations[0].Text == "")
                        {
                            ReadOnlyCollection<IWebElement> back_2 = driver.FindElements(By.CssSelector(".FormSection__PlaceholderFieldValue"));
                            /*Thread.Sleep(300);*/
                            back_2[2].Click();
                        }

                        generations = radioList_2[1].FindElements(By.CssSelector(".Radio__text"));
                        /*Thread.Sleep(500);*/

                        List<string> generationList = new List<string>();

                        // Перебор поколений
                        foreach (var generation in generations)
                        {
                            var gen = generation.Text;
                            if (gen.Contains("\u0420\u0435\u0441\u0442\u0430\u0439\u043B\u0438\u043D\u0433"))
                                gen = gen.Replace("\u0420\u0435\u0441\u0442\u0430\u0439\u043B\u0438\u043D\u0433", "Рестайлинг");
                            if (gen.Contains("\u043D.\u0432."))
                                gen = gen.Replace("\u043D.\u0432.", null);
                            generationList.Add(gen);
                        }

                        carModels.Add(new CarModel
                        {
                            Model = modelName,
                            Generations = generationList
                        });

                        // Возвращение к списку моделей
                        ReadOnlyCollection<IWebElement> back = driver.FindElements(By.CssSelector(".FormSection__PlaceholderFieldValue"));
                        Thread.Sleep(200);
                        back[1].Click();
                        Thread.Sleep(200);
                    }

                    CarMake carMake = new CarMake
                    {
                        Make = makeName,
                        Models = carModels
                    };

                    // Сериализация текущей марки автомобилей в JSON строку
                    string jsonString = JsonConvert.SerializeObject(carMake, Formatting.Indented);

                    // Запись JSON строки в файл
                    File.WriteAllText("manufacturer_" + makeName + ".json", jsonString, Encoding.UTF8);

                    Console.WriteLine($"Данные успешно сохранены в manufacturer_{makeName}.json");

                    // Возвращение к списку марок
                    ReadOnlyCollection<IWebElement> backToMakes = driver.FindElements(By.CssSelector(".FormSection__PlaceholderFieldValue"));
                    backToMakes[0].Click();
                }
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Произошла ошибка: " + e.Message);
            }
            finally
            {
                // Закрытие браузера
                driver.Quit();
            }
        }
    }
}
