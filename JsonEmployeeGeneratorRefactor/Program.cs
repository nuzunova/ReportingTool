using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JsonEmployeeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            List<JsonEmployee> employees;
            try
            {
                employees = ReadEmployeeData("employees.txt");
            }
            catch(Exception ex)
            {
                Console.WriteLine(string.Format("Error reading data. Details: {0}",ex.Message));
                Console.ReadLine();
                return;
            }

            try
            {
                WriteEmployeesAsJsonFile("employees.json", employees);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error writing data. Details: {0}", ex.Message));
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Finished successfully.");
            Console.ReadLine();
        }

        private static List<JsonEmployee> ReadEmployeeData(string fileName)
        {
            Random generator = new Random();
            string[] roles = new string[] { "Junior Developer", "Semi Senior Developer", "Senior Developer", "Principal", "Team Leader" };
            string[] teams = new string[] { "Platform", "Sales", "Billing", "Mirage" };

            var employees = new List<JsonEmployee>();

            //Read and parse data 
            var all_lines_in_file = File.ReadAllLines("employees.txt").ToArray();
            for (var i = 0; i < all_lines_in_file.Length; i++)
            {
                string line = all_lines_in_file[i];
                var arr = line.Split('\t');
                if (arr.Length < 3)
                {
                    throw new Exception("Inceorrect employee format");
                }

                JsonEmployee e = new JsonEmployee();
                e.Id = i;
                e.Name = arr[0];
                e.SurName = arr[1];
                e.Email = arr[2];
                e.Age = generator.Next(18, 66);
                if (i < 11)
                {
                    e.Role = "Manager";
                    e.Teams = new List<string>();
                }
                else
                {
                    e.ManagerId = generator.Next(11);
                    e.Role = roles[generator.Next(4)];
                    int count = generator.Next(1, 4);
                    var employeeTeams = new List<string>();
                    for (int j = 0; j < count; ++j)
                    {
                        employeeTeams.Add(teams[generator.Next(4)]);
                    }
                    e.Teams = employeeTeams;
                }

                i++;
                employees.Add(e);
            }

            return employees;
        }

        private static void WriteEmployeesAsJsonFile(string fileName, List<JsonEmployee> employees)
        {
            using (var jsonFile = File.CreateText(fileName))
            {
                jsonFile.WriteLine("[");
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < employees.Count; i++)
                {
                    builder.Clear();
                    var jsonEmployee = employees[i];

                    builder.Append(string.Format("{{\"Id\":{0}", jsonEmployee.Id));
                    builder.Append(",");
                    builder.Append(string.Format("\"ManagerId\":{0}", jsonEmployee.ManagerId.HasValue ? jsonEmployee.ManagerId.ToString() : "null"));
                    builder.Append(",");
                    builder.Append(string.Format("\"Age\":{0}", jsonEmployee.Age));
                    builder.Append(",");

                    builder.Append(string.Format("\"Teams\":{0}", string.Join(",", jsonEmployee.Teams.Select(x => "\"" + x + "\""))));
                    builder.Append(",");
                    builder.Append(string.Format("\"Role\":{0}", jsonEmployee.Role));
                    builder.Append(",");
                    builder.Append(string.Format("\"Email\":{0}", jsonEmployee.Email));
                    builder.Append(",");
                    builder.Append(string.Format("\"SurName\":{0}", jsonEmployee.SurName));
                    builder.Append(",");
                    builder.Append(string.Format("\"Name\":{0}}}", jsonEmployee.Name));
                    builder.Append(",");

                    if (i != employees.Count - 1)
                        builder.Append(",");
                    var formattedEmployeed = builder.ToString();

                    jsonFile.WriteLine(formattedEmployeed);
                }
                jsonFile.WriteLine("]");
                jsonFile.Flush();
            }
        }

    }

}
