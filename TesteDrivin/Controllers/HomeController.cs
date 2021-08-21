using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TesteDrivin.Models;

namespace TesteDrivin.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ListarEmail(List<IFormFile> files)
        {
            if (files.Any())
            {
                DeletarArquivo();
                var arquivo = ObterArquivo(files[0]);
                var listaEmail = LerArquivo(arquivo);
                var listaEmailParaSalvar = new List<string>();
                int x = 0;
                for (int i = 0; i < listaEmail.Count; i++)
                {
                    string item = listaEmail[i];
                    listaEmailParaSalvar.Add(item);
                    if (listaEmailParaSalvar.Count == 5)
                    {
                        x++;
                        var arquivoDeEmail = string.Join(", ", listaEmailParaSalvar.ToArray());
                        GravarArquivo(arquivoDeEmail, x);
                        listaEmailParaSalvar.Clear();
                    }

                    if (i == listaEmail.Count - 1 && listaEmailParaSalvar.Any())
                    {
                        x++;
                        var arquivoDeEmail = string.Join(", ", listaEmailParaSalvar.ToArray());
                        GravarArquivo(arquivoDeEmail, x);
                        listaEmailParaSalvar.Clear();
                    }
                }

                ViewBag.ArquivoInvalido = "Arquivo salvo com sucesso!";
                return View("Index");
            }

            ViewBag.ArquivoInvalido = "Arquivo invalido! Ta Achando que sou a Renner rsrsrs!!";
            return View("Index");
        }

        public IActionResult ListarArquivo()
        {
            var lstArquivoViewModel = LerArquivo();
            return View(lstArquivoViewModel);
        }

        public IActionResult BaixarArquivo(string nomeArquivo)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Arquivo", nomeArquivo);
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/force-download", nomeArquivo);
        }

        public bool ValidarEmail(string email)
        {
            Regex regex = new(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(email);
            return match.Success;
        }

        public void GravarArquivo(string arquivo, int x)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(arquivo);
            var fileName = $"{x}-{DateTime.Now:dd-MM-yyyy}.txt";
            MemoryStream ms = new(buffer);
            FileStream file = new(Path.Combine(Directory.GetCurrentDirectory(), "Arquivo", fileName), FileMode.Create, FileAccess.Write);
            ms.WriteTo(file);
            file.Close();
            ms.Close();
        }

        public void DeletarArquivo()
        {
            var directoryInfo = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Arquivo"));
            foreach (FileInfo fi in directoryInfo.GetFiles())
            {
                fi.Delete();
            }
        }

        public List<string> LerArquivo(MemoryStream arquivo)
        {
            List<string> lstEmail = new();
            var fileBytes = arquivo.ToArray();
            using StreamReader linha = new(new MemoryStream(fileBytes));
            while (!linha.EndOfStream)
            {
                string email = linha.ReadLine();
                if (ValidarEmail(email) && !lstEmail.Contains(email))
                {
                    lstEmail.Add(email);
                }
            }
            return lstEmail;
        }

        public List<ArquivoViewModel> LerArquivo()
        {
            List<ArquivoViewModel> lstArquivoViewModel = new();
            string[] arquivos = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Arquivo"));
            foreach (string arq in arquivos)
            {
                FileInfo infoArquivo = new(arq);
                using StreamReader linha = new(arq);
                var arquivoViewModel = new ArquivoViewModel
                {
                    Nome = infoArquivo.Name
                };
                arquivoViewModel.Emails = new List<string>();
                while (!linha.EndOfStream)
                {
                    string email = linha.ReadLine();
                    arquivoViewModel.Emails.Add(email);
                }
                lstArquivoViewModel.Add(arquivoViewModel);
            }

            return lstArquivoViewModel;
        }

        internal static MemoryStream ObterArquivo(IFormFile file)
        {
            var target = new MemoryStream();
            file.CopyTo(target);
            return target;
        }
    }
}
