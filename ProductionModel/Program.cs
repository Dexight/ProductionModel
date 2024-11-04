using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ProductionModel
{
    internal class Program
    {
        static void PrintSet(HashSet<string> collection)
        {
            Console.Write("    {");
            foreach (string i in collection)
            {
                Console.Write(" {0}", i);
            }
            Console.WriteLine(" }");
        }

        static HashSet<string> facts = new HashSet<string>();
        /// <summary>
        /// В доказательстве каких теорем участвует данный факт(ключ).
        /// </summary>
        static Dictionary<string, HashSet<string>> forward_productions = new Dictionary<string, HashSet<string>>();
        /// <summary>
        /// Что доказывается, [...Что для этого необходимо...]
        /// </summary>
        static Dictionary<string, List<List<string>>> reverce_productions = new Dictionary<string, List<List<string>>>();

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

                    if (!reverce_productions.ContainsKey(right))
                        reverce_productions[right] = new List<List<string>>();
                    reverce_productions[right].Add(left);
                    
                    // для прямого поиска
                    foreach (string s in left)
                    {
                        if (!forward_productions.ContainsKey(s))
                            forward_productions[s] = new HashSet<string>();
                        forward_productions[s].Add(right);

                    }

                    facts.Add(right);
                }
                Console.WriteLine($"Log: Найдено {reverce_productions.Count} продукций (с повторами) и {facts.Count} уникальных элементов.");
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
                Console.WriteLine($"Log: Описано {description.Count} уникальных элементов.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА при чтении файла: {ex.Message}");
            }
        }

        static void PrintWay(Dictionary<int, Dictionary<string, List<string>>> output, int depth, string need)
        {
            HashSet<string> proved = new HashSet<string> { need };

            while (depth > 0)
            {
                Console.WriteLine($"\n{depth} слой");
                HashSet<string> shouldWatch = new HashSet<string>();
                foreach (string s in proved)
                {
                    if (output[depth].ContainsKey(s))
                    {
                        int i = 0;
                        foreach (string ss in output[depth][s])
                        {
                            Console.Write(ss);
                            shouldWatch.Add(ss);
                            if (i < output[depth][s].Count - 1)
                                Console.Write(" & ");
                            ++i;
                        }
                        Console.WriteLine(" => " + s);
                    }
                    else shouldWatch.Add(s);
                }
                proved = shouldWatch.ToHashSet();// по значению
                depth--;
            }
        }

        static void ForwardSearch(HashSet<string> getted_facts, string need)
        {
            int debug_input_count = getted_facts.Count;
            Dictionary<int, Dictionary<string, List<string>>> output = new Dictionary<int, Dictionary<string, List<string>>>(); //глубина => список продукций вида (доказанное -> из чего)

            if (getted_facts.Contains(need))
            {
                Console.WriteLine($"'{need}:{description[need]}' <=> {need}:'{description[need]}'.\nГлубина 0."); 
                return;
            }

            int depth = 0;
            HashSet<string> newFacts = new HashSet<string>();
            while (true)
            {
                ++depth;
                //Console.WriteLine(depth + " " + newFacts.Count);// DEBUG

                bool can_continue = false; //будет true, если хотя бы 1 теорема была доказана
                foreach (string s in getted_facts)
                {
                    if (!forward_productions.ContainsKey(s))
                        continue;
                    List<string> can_goto = forward_productions[s].ToList();// в док-ве каких правых частей участвует 's'
                    
                    // Проходим по кандидатным правилам
                    foreach (string cgt in can_goto)
                    {
                        if (getted_facts.Contains(cgt)) continue;// если такой факт уже имеется/доказан
                        if (!reverce_productions.ContainsKey(cgt)) continue;

                        List<List<string>> founded_productions = reverce_productions[cgt];

                        // есть ли всё необходимое для одного из док-в
                        List<string> left = new List<string>();
                        if (founded_productions.Exists(lst => {
                            foreach (string f in lst)
                                if (!getted_facts.Contains(f)) return false;
                            left = lst.ToList();//.ToList() для передачи по значению
                            return true;
                        }))
                        {
                            //если теорема доказана
                            reverce_productions.Remove(cgt);// удаляем правила вывода этого факта из общего списка продукций
                            List<string> should_delete_keys = forward_productions.Where(elem => elem.Value.Contains(cgt)).Select(elem => elem.Key).ToList();
                            foreach(string key in should_delete_keys)
                                forward_productions[key].Remove(cgt);
                            newFacts.Add(cgt);

                            if (!output.ContainsKey(depth))
                                output[depth] = new Dictionary<string, List<string>>();
                            output[depth][cgt] = left; //на этой глубине мы доказали теорему cgt используя элементы из left
                            can_continue = true;
                        }
                    }
                }

                //Если нашли то, что нужно
                if (newFacts.Contains(need))
                {
                    Console.WriteLine($"ДОКАЗАТЕЛЬСТВО ВОЗМОЖНО. Глубина вывода - {depth}");
                    PrintWay(output, depth, need);
                    return;
                }

                if (!can_continue)
                {
                    //Console.WriteLine("    LOG: getted_facts.Count = " + getted_facts.Count + $" - {debug_input_count}(стартовые факты) = newFacts.Count = " + newFacts.Count + ";");
                    //PrintSet(getted_facts);
                    //PrintSet(newFacts);
                    //Console.Write($"\nПоиск прекращён на {depth} глубине. ");
                    Console.WriteLine($"ДОКАЗАТЕЛЬСТВО НЕВОЗМОЖНО.\n");
                    return;
                }    

                getted_facts.UnionWith(newFacts);
            }
        }

        static void ReverceSearch(HashSet<string> getted_facts, string need)
        {
            if (getted_facts.Count == 1 && getted_facts.Contains(need))
            {
                Console.WriteLine($"'{need}' <=> '{need}'.\nГлубина 0.");
                return;
            }

            int depth = 0;
            HashSet<HashSet<string>> variations = new HashSet<HashSet<string>> ();// деревья возможных решений
            HashSet<string> first = new HashSet<string> {need};
            variations.Add(first);

            while (true)
            {
                ++depth;
                HashSet<HashSet<string>> new_variations = new HashSet<HashSet<string>>();// новое дерево возможных решений

                HashSet<HashSet<string>> new_vv;
                foreach (HashSet<string> v in variations)
                {
                    new_vv = new HashSet<HashSet<string>> ();
                    foreach (string fact in v)
                    {
                        List<List<string>> left = reverce_productions[fact];//варианты левых частей продукций для fact
                           
                    }
                }

                variations.Clear();
                variations = new_variations.ToHashSet();
            }
        }

        static void Main()
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
            HashSet<string> getted_facts_hs = (getted_facts.ToHashSet());
            // Алгоритмы
            if (search_method == "1")
                ForwardSearch(getted_facts_hs, need);
            else
                ReverceSearch(getted_facts_hs, need);
        }
    }
}