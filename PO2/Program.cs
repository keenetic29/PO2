using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.IO;

namespace PO2
{
  class Program
  {
    static void Main(string[] args)
    {

      Console.WriteLine("Введите 3 хэш значения:");
      string h1 = Console.ReadLine();
      string h2 = Console.ReadLine();
      string h3 = Console.ReadLine();
      string[] hashes = { h1, h2, h3 };

      Console.Write("Введите кол-во потоков: ");
      int numThreads = int.Parse(Console.ReadLine());

      Console.WriteLine("Начался процесс нахождения паролей:");

      Stopwatch stopwatch = Stopwatch.StartNew();

      char[] alphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
      int passwordLength = 5;
      int totalPasswordCount = (int)Math.Pow(alphabet.Length, passwordLength);

      Console.WriteLine($"Кол-во паролей для проверки: {totalPasswordCount}");

      /*
      Используем Parallel.ForEach, чтобы распараллелить перебор паролей. 
      Мы создаем разделитель (partitioner), который делит диапазон от 0 до общего количества паролей 
      на несколько поддиапазонов, чтобы их можно было обработать параллельно. 
      Мы также указываем максимальное количество потоков (MaxDegreeOfParallelism), 
      которое получаем от пользователя. range представляет текущий поддиапазон, который будет обработан 
      в каждом потоке.
       */
      Parallel.ForEach(Partitioner.Create(0, totalPasswordCount), new ParallelOptions { MaxDegreeOfParallelism = numThreads },
          (range, state) =>
          {

            /*
            Здесь мы перебираем все возможные пароли в текущем поддиапазоне. 
            Мы генерируем каждый пароль с помощью GetPasswordFromIndex, 
            используя текущий индекс (число от range.Item1 до range.Item2) и передавая алфавит, длину пароля. 
            Затем мы вычисляем хэш-значение для каждого пароля с помощью CalculateSHA256 и проверяем, 
            есть ли это хэш-значение в массиве hashes. Если пароль найден, мы выводим его в консоль.
             */
            for (int i = range.Item1; i < range.Item2; i++)
            {
              string password = GetPasswordFromIndex(i, alphabet, passwordLength);
              string hashedPassword = CalculateSHA256(password);

              if (Array.IndexOf(hashes, hashedPassword) != -1)
              {
                Console.WriteLine($"Пароль найден: {password}");
              }
            }
          });


      stopwatch.Stop();

      Console.WriteLine($"Времени потребовалось: {stopwatch.Elapsed}");


      Console.ReadKey();
    }

    /*
     Вычисляется остаток от деления index на длину алфавита (symbolIndex), чтобы получить индекс символа в алфавите.
     Символ с индексом symbolIndex добавляется в начало passwordBuilder с помощью метода Insert(0, ...), 
     чтобы символы пароля были в правильном порядке.
     Затем index делится на длину алфавита, чтобы сдвинуться к следующему символу пароля.
     */
    static string GetPasswordFromIndex(long index, char[] alphabet, int passwordLength)
    {
      StringBuilder passwordBuilder = new StringBuilder(passwordLength);

      for (int i = 0; i < passwordLength; i++)
      {
        int symbolIndex = (int)(index % alphabet.Length);
        passwordBuilder.Insert(0, alphabet[symbolIndex]);
        index /= alphabet.Length;
      }

      return passwordBuilder.ToString();
    }

    static string CalculateSHA256(string input)
    {
      using (SHA256 sha256 = SHA256.Create())
      {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = sha256.ComputeHash(inputBytes);
        return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
      }
    }
  }
}
