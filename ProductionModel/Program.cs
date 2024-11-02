using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.OleDb;
using System.Runtime.CompilerServices;

namespace ProductionModel
{
    internal class Program
    {
        static HashSet<string> facts = new HashSet<string>();

        static HashSet<(string, List<string>)> forward_productions = new HashSet<(string, List<string>)>();// Откуда и что можно доказать.
        static HashSet<(string, List<string>)> reverce_productions = new HashSet<(string, List<string>)>();// Что доказывается, [Что для этого необходимо]

        static Dictionary<string, string> description = new Dictionary<string, string>();

        static void parseProductions()
        {
            string filePath = "../../model.txt";

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string[] parsed = line.Split(' ');
                    
                    if (parsed.Length == 1) // для аксиом
                    {
                        facts.Add(parsed[0]);
                        continue;
                    }

                    // для обратного поиска
                    string right = parsed[parsed.Length - 1];
                    List<string> left = new List<string>();
                    for (int i = 0; i < parsed.Length - 1; i++)
                    {
                        if (parsed[i] == "=>" || parsed[i] == "&")
                            continue;
                        left.Add(parsed[i]);
                    }
                    reverce_productions.Add((right, left));
                    
                    // для прямого поиска
                    foreach (string s in left)
                    {
                        
                    }

                    facts.Add(right);
                }
                Console.WriteLine($"Log: Найдено {reverce_productions.Count} продукций и {facts.Count} фактов.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА при чтении файла: {ex.Message}");
            }
        }

        static void parseDescription()
        {
            string filePath = "../../description.txt";

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string[] parsed = line.Split(' ');
                    string descr = string.Join(" ", parsed.Skip(1));
                    description[parsed[0]] = descr;
                }
                Console.WriteLine($"Log: Описано {description.Count} фактов.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА при чтении файла: {ex.Message}");
            }
        }

        static void ForwardSearch(string[] getted_facts, string need)
        {
            if (getted_facts.Length == 1 && getted_facts[0] == need)
            {
                Console.WriteLine($"'{description[need]}' <=> '{description[need]}'.\nГлубина 0."); 
                return;
            }


        }

        static void ReverceSearch(string[] getted_facts, string need)
        {
            if (getted_facts.Length == 1 && getted_facts[0] == need)
            {
                Console.WriteLine($"'{description[need]}' <=> '{description[need]}'.\nГлубина 0.");
                return;
            }

        }

        static void Main(string[] args)
        {
            //чтение из файлов
            parseProductions();
            parseDescription();

            //Ввод пользователя
            //Набор фактов
            string[] getted_facts;
            Console.WriteLine("Введите (через пробел) те факты (A1-15 и T1-105), что мы имеем. \nНапример, если у нас есть аксиомы А1, А5 и доказана теорема Т8, то мы вводим: 'А1 А5 Т8'\n");

            while (true)
            {
                string cur_facts = Console.ReadLine();

                Console.WriteLine();
                Console.WriteLine($"    Принято на вход: '{cur_facts}'");
                Console.WriteLine();
                getted_facts = cur_facts.Split(' ');

                bool flag = false;
                for (int i = 0; i < getted_facts.Length; i++)
                {
                    if (!facts.Contains(getted_facts[i]))
                    {
                        Console.WriteLine($"    ОШИБКА: факт {getted_facts[i]} не найден.\n\nВведите ещё раз:\n");
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                    break;
            }
                //Что нужно доказать
            string need;

            Console.WriteLine("Введите, что требуется доказать.\nНапример: 'T10'\n");
            
            while (true)
            {
                need = Console.ReadLine();

                Console.WriteLine();
                Console.WriteLine($"    Принято на вход: '{need}'");
                Console.WriteLine();

                if (!facts.Contains(need))
                {
                    Console.WriteLine($"    ОШИБКА: Название {need} не найдено в списке доступных.\n\nВведите ещё раз:\n");
                    continue;
                }
                break;
            }

            // Выбор - прямой или обратный поиск
            string search_method;
            Console.WriteLine("Введите, нужен:\n1: Прямой поиск\nили\n2: Обратный поиск:\n");
            while (true)
            {
                search_method = Console.ReadLine();

                Console.WriteLine();
                Console.WriteLine($"    Принято на вход: '{search_method}'");
                Console.WriteLine();

                if (search_method == "1" || search_method == "2") break;
                Console.WriteLine($"    ОШИБКА: Ввод {search_method} некорректен.\n\nВведите ещё раз:\n\n");
            }

            // Постановка задачи
            Console.WriteLine($"\nКонец пользовательского ввода.\n\nЗадача: {string.Join(", ", getted_facts)} => {need} с помощью {(search_method == "1" ? "прямого" : "обратного")} поиска.\n\n");

            // Алгоритмы
            if (search_method == "1")
                ForwardSearch(getted_facts, need);
            else
                ReverceSearch(getted_facts, need);
        }
    }
}
