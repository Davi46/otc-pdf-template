using Microsoft.Extensions.DependencyInjection;
using Otc.PdfTemplate.Abstractions;
using Otc.PdfTemplate.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Otc.PdfTemplate.Tests
{
    public class PdfTemplateBind
    {
        private readonly ServiceProvider serviceProvider;

        private IPdfGenerator pdfConverter;

        public PdfTemplateBind()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddScoped<IPdfGenerator, PdfGenerator>();

            serviceProvider = services.BuildServiceProvider();
        }


        [Fact]
        public void Make_Data_And_Images_Merge_With_Template()
        {
            pdfConverter = serviceProvider.GetService<IPdfGenerator>();

            var dictionary = BuildDictionaryForImage();
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "TemplateBoleto.pdf");

            Assert.True(pdfConverter.AddRange(dictionary)
                            .AddImage(new BarcodeGenerator().GenerateBarcode("03399000000000000009762852800000733268360101"), 50, 465)
                            .PathFile(templatePath)
                            .Generate() != null);
        }

        [Fact]
        [Trait(nameof(PdfAGererator.Generate), "Sucesso")]
        public void PdfAGenerateAsync_Sucesso()
        {
            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("N_Termo", "012"));
            parameters.Add(new KeyValuePair<string, string>("Nome", "Teste"));
            parameters.Add(new KeyValuePair<string, string>("CPF", "01205524585"));
            parameters.Add(new KeyValuePair<string, string>("Beneficio", "0128787"));
            parameters.Add(new KeyValuePair<string, string>("codigo", "012"));

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "TemplatePdfA.pdf");

            var pdfAGerator = new PdfAGererator();

            pdfAGerator.PathFile(templatePath);
            pdfAGerator.AddRange(parameters);

            var pdfa = pdfAGerator.Generate();

            var targetFile = "Saida.pdf";

            using (var sourceStream = new MemoryStream(pdfa))
            using (var saida = new FileStream(targetFile, FileMode.OpenOrCreate))
            {
                sourceStream.CopyTo(saida);
            }

            Assert.True(pdfa != null);
        }

        [Fact]
        [Trait(nameof(PdfAGererator.Generate), "ArgumentNullException")]
        public void PdfAGenerateAsync_TemplatePathInvalido()
        {
            var paraNameExpected = "_templatePath";
            var menssageExpected = "Value cannot be null.";

            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("N_Termo", "012"));
            parameters.Add(new KeyValuePair<string, string>("Nome", "Teste"));
            parameters.Add(new KeyValuePair<string, string>("CPF", "01205524585"));
            parameters.Add(new KeyValuePair<string, string>("Beneficio", "0128787"));
            parameters.Add(new KeyValuePair<string, string>("codigo", "012"));


            var pdfAGerator = new PdfAGererator();
            pdfAGerator.AddRange(parameters);

            var ex = Assert.ThrowsAny<ArgumentNullException>(() =>
            {
                pdfAGerator.Generate();
            });

            Assert.Equal(paraNameExpected, ex.ParamName);
            Assert.Contains(menssageExpected, ex.Message);
        }

        [Fact]
        [Trait(nameof(PdfAGererator.Generate), "PdfTemplateException")]
        public void PdfAGenerateAsync_NumeroParametroInvalido()
        {
            var messageExpected = "N�mero de par�metros de entrada de dados diferente do n�mero de par�metros do template";

            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("Teste", "012"));

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "TemplatePdfA.pdf");

            var pdfAGerator = new PdfAGererator();
            pdfAGerator.PathFile(templatePath);
            pdfAGerator.AddRange(parameters);

            var ex = Assert.ThrowsAny<PdfTemplateException>(() =>
            {
                pdfAGerator.Generate();
            });

            Assert.Equal(messageExpected, ex.Message);
        }

        [Fact]
        [Trait(nameof(PdfAGererator.Generate), "PdfTemplateException")]
        public void PdfAGenerateAsync_ParametroInvalido()
        {
            var messageExpected = "Os par�metros [Teste] n�o existem no template";

            var parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("Nome", "Teste"));
            parameters.Add(new KeyValuePair<string, string>("CPF", "01205524585"));
            parameters.Add(new KeyValuePair<string, string>("Teste", "012"));
            parameters.Add(new KeyValuePair<string, string>("Beneficio", "0128787"));
            parameters.Add(new KeyValuePair<string, string>("codigo", "012"));

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "TemplatePdfA.pdf");

            var pdfAGerator = new PdfAGererator();
            pdfAGerator.PathFile(templatePath);
            pdfAGerator.AddRange(parameters);

            var ex = Assert.ThrowsAny<PdfTemplateException>(() =>
            {
                pdfAGerator.Generate();
            });

            Assert.Equal(messageExpected, ex.Message);
        }

        #region private

        private Dictionary<string, string> BuildDictionary()
        {
            Dictionary<string, string> formModel = new Dictionary<string, string>
            {
                {"Nome", "Ze Ruela da Silva"},
                {"CPF", "01234567890"},
                {"Identidade", "12457"},
                {"Endereco", "rua do nada"},
                {"N", "5"},
                {"Complemento", "nada"},
                {"Bairro", "Tabajara"},
                {"Cidade", "Dazueira"},
                {"UF", "KK"},
                {"CEP", "456789"},
            };

            return formModel;
        }

        private Dictionary<string, string> BuildDictionaryForImage()
        {
            Dictionary<string, string> contractPaymentModel = new Dictionary<string, string>
            {
                { "N_BANCO", "347" },
                { "PREFIXO", "033-7" },
                { "LINHA_DIGITAVEL", "03399.76284 52800.000730 40529.901015 7 0000000" },
                { "PAGADOR", "Ze RUELA DA SILVA   010.695.984-02\nRUA VAI QUE COLA, 1726,   StO Cristo\nTabajara - RN \nCEP: 59615270   " },
                { "NOSSO_NUMERO", "000.000.000-42" },
                { "NUMERO_DOCUMENTO", "000.000.000-42" },
                { "DT_VENCIMENTO", "17/08/2018" },
                { "VLR_DOCUMENTO", "35.656,64" },
                { "BENEFICIARIO", "00.000-042" },
                { "AGENCIA_BENEFICIARIA", "181" },
                { "N_BANCO2", "347" },
                { "PREFIXO2", "033-7" },
                { "LINHA_DIGITAVEL2", "03399.76284152800.000730140529.9010151710000000" },
                { "AGENCIA_RECEBEDORA", "Sei nao" },
                { "DT_VENCIMENTO2", "17/08/2018" },
                { "BENEFICIARIO2", "00.000-042" },
                { "AGENCIA_BENEFICIARIA2", "\n181" },
                { "DT_DOCUMENTO", "Yes" },
                { "NUMERO_DOCUMENTO2", "000.000.000-42" },
                { "NOSSO_NUMERO2", "000.000.000-42" },
                { "DT_PROCES", "Yes" },
                { "VALOR", "Yes" },
                { "VLR_DOCUMENTO2", "35.656,64" },
                { "INSTRUCOES", "Yes" },
                { "PAGADOR2", "Ze RUELA DA SILVA   000.000.000-42\nRUA VAI QUE COLA, 1726,   StO Cristo\nTabajara - RN \nCEP: 59615270   " }
            };

            return contractPaymentModel;
        }

        #endregion
    }
}