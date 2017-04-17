using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KnxNetCore.Telegrams;
using Xunit;

namespace KnxNetCore.UnitTests.Telegrams
{
    public class GroupAddressTests
    {
        [Fact]
        public void UShort_address_775_is_0_3_7()
        {
            var sut = new GroupAddress(775);
            var result = sut.As3Level;
            Assert.Equal(new byte[] { 0, 3, 7 }, result);
        }

        [Fact]
        public void Address_0_3_7_is_ushort_address_775()
        {
            var sut = new GroupAddress(0, 3, 7);
            Assert.Equal(sut.AsUShort, 775);
        }

        [Fact]
        public void UShort_address_40577_is_12_6_129()
        {
            var sut = new GroupAddress(40577);
            var result = sut.As3Level;
            Assert.Equal(new byte[] { 19, 6, 129 }, result);
        }

        [Fact]
        public void Address_12_6_129_is_ushort_address_40577()
        {
            var sut = new GroupAddress(19, 6, 129);
            Assert.Equal(sut.AsUShort, 40577);
        }

        [Fact]
        public void Address_with_main_group_33_cannot_be_constructed()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GroupAddress(33, 0, 0));
        }

        [Fact]
        public void Address_with_middle_group_10_cannot_be_constructed()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GroupAddress(0, 10, 0));
        }
    }
}
