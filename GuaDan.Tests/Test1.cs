using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GuaDan;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuaDan.Tests
{
    [TestClass]
    public class FrmGuaDanTests
    {
        [TestMethod]
        public void GroupByFrequency_ShouldReturnCorrectResult_WhenInputIsValid()
        {
            // Arrange
            FrmGuaDan frmGuaDan = new FrmGuaDan();
            List<string> input = new List<string> { "1-2", "2-3", "1-3", "1-4", "2-4" };
           
            // Act
            var result = frmGuaDan.GroupByFrequency(input);
            Assert.IsNotNull(result);
            // Assert
            var expected = new List<string> { "1-2,3,4", "2-3,4" };
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GroupByFrequency_ShouldReturnEmptyList_WhenInputIsEmpty()
        {
            // Arrange
            var frmGuaDan = new FrmGuaDan();
            var input = new List<string>();

            // Act
            var result = frmGuaDan.GroupByFrequency(input);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GroupByFrequency_ShouldHandleInvalidPairsGracefully()
        {
            // Arrange
            var frmGuaDan = new FrmGuaDan();
            var input = new List<string> { "1-2", "invalid", "3-4" };

            // Act
            var result = frmGuaDan.GroupByFrequency(input);

            // Assert
            var expected = new List<string> { "1-2", "3-4" };
            CollectionAssert.AreEqual(expected, result);
        }
    }
}