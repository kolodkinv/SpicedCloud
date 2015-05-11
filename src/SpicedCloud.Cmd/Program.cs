using System;
using System.Diagnostics;
using System.Threading;
using System.Web;
using System.IO;
using Dropbox.Api;

namespace SpicedCloud.Cmd
{
    class Program
    {

        public static void line()
        {
            Console.WriteLine("-------------------------------------------------------------------------------");
        }

        public static void showAccount(Account acc)
        {
            Console.WriteLine("{0, 20}{1, 30}", "Имя Фамилия ", acc.DisplayName);
            Console.WriteLine("{0,20}{1,30}", "Email ", acc.Email);
            Console.WriteLine("{0,20}{1,30}", "Страна ", acc.Country);
        }

        public static void help()
        {
            line();
            Console.WriteLine("\t VIEW_FILES - посмотреть  все файлы в хранилище");
            Console.WriteLine("\t CREATE_FOLDER - создать папку в хранилище");
            Console.WriteLine("\t DELETE - удалить файл или папку");
            Console.WriteLine("\t ADD_FILE - добавить файл в хранилище");
            Console.WriteLine("\t DOWNLOAD_FILE - загрузить файл на жесткий диск");
            Console.WriteLine("\t EXIT - выйти из программы");
            line();
        }


        public static void delete(DropboxApi api, DropboxClient client)
        {
            string name = "";
            Console.WriteLine("Введите название удаляемого объекта ");
            name = Console.ReadLine();

            var del = client.delete(api, name);



            Console.WriteLine("\t\tУдалено");
            Console.WriteLine("{0,20}{1,30}", "Путь ", del.Root + del.Path);
            Console.WriteLine("{0,20}{1,30}", "Удалено ", del.Modified);

        }

        public static void createNewFolder(DropboxApi api, DropboxClient client)
        {
            string name = "";

            Console.WriteLine("Введите название папки ");
            name = Console.ReadLine();


            var folder = client.createFolder(api, name);

            Console.WriteLine("\t\tПапка создана");
            Console.WriteLine("{0,20}{1,30}", "Путь ", folder.Root + folder.Path);
            Console.WriteLine("{0,20}{1,30}", "Изменено ", folder.Modified);
        }

        public static void addFile(DropboxApi api, DropboxClient client)
        {
            string name = "";
            string path = "";

            Console.WriteLine("Введите полный путь добавляемого файла");
            path = Console.ReadLine();


            string[] spl = path.Split('\\');

            foreach (string s in spl)
            {

                if (s.Trim() != "")
                    name = s;

            }
           
            Console.WriteLine("Name= " + name);


            var folder = client.addFile(api, name, path);

            Console.WriteLine("\t\tФайл добавлен");
            Console.WriteLine("{0,20}{1,30}", "Путь ", folder.Root + folder.Path);
            Console.WriteLine("{0,20}{1,30}", "Изменено ", folder.Modified);
        }


        public static void downloadFile(DropboxApi api, DropboxClient client)
        {
            string name = "";
            string path = "";

            Console.WriteLine("Введите название загружаемого файла");
            name = Console.ReadLine();



            Console.WriteLine("Name= " + name);



            var folder = client.downloadFile(api, name);

            string[] spl = name.Split('\\');

            foreach (string s in spl)
            {

                if (s.Trim() != "")
                    path = s;

            }
            Console.WriteLine("Name= " + path);

            folder.Save(path);

            Console.WriteLine("\t\tФайл добавлен");
            Console.WriteLine("{0,20}{1,30}", "Путь ", folder.Root + folder.Path);
            Console.WriteLine("{0,20}{1,30}", "Изменено ", folder.Modified);
        }

        public static void getFiles(DropboxApi api, DropboxClient client)
        {
            string name = "";


            Console.WriteLine("Введите название папки");
            name = Console.ReadLine();


            var folder = client.viewFiles(api, name);
            foreach (var file in folder.Contents)
            {
                Console.WriteLine(file.Path);
            }
        }

        static void Main()
        {

            Console.WriteLine("{0,45}", "DropBox Client");
            line();

            DropboxClient client = new DropboxClient();

            DropboxApi api = client.connectDropbox("KsHXYcj-h_AAAAAAAAAAUr3Ebnrxv-Hrm8Vn7DYRZ5iEA3pw7xZG5jUs8E2UctRZ");


            var account = client.getInfoAccount(api);



            Console.WriteLine("{0,20}{1}", "Здравствуйте ", account.DisplayName);

            string command = "";

            help();

            while (true)
            {
                command = Console.ReadLine();

                switch (command)
                {
                    case "VIEW_FILES": getFiles(api, client);
                        break;
                    case "CREATE_FOLDER": createNewFolder(api, client);
                        break;
                    case "DELETE": delete(api, client);
                        break;
                    case "ADD_FILE": addFile(api, client);
                        break;
                    case "DOWNLOAD_FILE": downloadFile(api, client);
                        break;
                    default:
                        Console.WriteLine("Nothing");
                        break;
                }
            }
        }
    }
}
