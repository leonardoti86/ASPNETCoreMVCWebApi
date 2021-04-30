using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MimicAPI.Database;
using MimicAPI.v1.Models;
using MimicAPI.v1.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mimic.Repository.Tests
{
    [TestClass]
    public class PalavraRepositoryTest
    {
        [TestMethod]
        public void TentaCadastrarUmaPalavra()
        {
            var mockSet = new Mock<DbSet<Palavra>>();
            var mockContext = new Mock<MimicContext>();
            mockContext.Setup(p => p.Palavras).Returns(mockSet.Object);

            var palavraRepository = new PalavraRepository(mockContext.Object);
            Palavra palavra = new Palavra()
            {
                Ativo = true,
                Criado = DateTime.Now,
                Nome = "GatoTesteUnitario",
                Pontuacao = 1
            };

            palavraRepository.Cadastrar(palavra);

            mockSet.Verify(m => m.Add(It.IsAny<Palavra>()), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void ObterPalavraDeId1()
        {
            var listaPalavras = new List<Palavra>
            {
                new Palavra { Id = 1, Nome = "urso", Ativo = true },
                new Palavra { Id = 2, Nome = "baleia", Ativo = false },
                new Palavra { Id = 3, Nome = "cobra", Ativo = true }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Palavra>>();
            mockSet.As<IQueryable<Palavra>>().Setup(p => p.Provider).Returns(listaPalavras.Provider);
            mockSet.As<IQueryable<Palavra>>().Setup(p => p.Expression).Returns(listaPalavras.Expression);
            mockSet.As<IQueryable<Palavra>>().Setup(p => p.ElementType).Returns(listaPalavras.ElementType);
            mockSet.As<IQueryable<Palavra>>().Setup(p => p.GetEnumerator()).Returns(listaPalavras.GetEnumerator());

            var mockContext = new Mock<MimicContext>();
            mockContext.Setup(p => p.Palavras).Returns(mockSet.Object);

            var palavraRepository = new PalavraRepository(mockContext.Object);
            
            var palavra = palavraRepository.Obter(1);

            Assert.AreEqual("urso", palavra.Nome);
        }
    }
}
