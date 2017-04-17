using System;
using KnxNetCore.Telegrams;
using Xunit;

namespace KnxNetCore.UnitTests.Telegrams
{
    public class IndividualAddressTests
    {
        [Fact]
        public void Address_0_1_2_is_ushort_address_258()
        {
            var sut = new IndividualAddress(0, 1, 2);
            Assert.Equal(258, sut.AsUShort);
        }

        [Fact]
        public void UShort_address_258_is_address_0_1_2()
        {
            var sut = new IndividualAddress(258);
            Assert.Equal(new byte[] { 0, 1, 2 }, sut.As3Level);
        }

        [Fact]
        public void Address_7_14_212_is_ushort_address_32468()
        {
            var sut = new IndividualAddress(7, 14, 212);
            Assert.Equal(32468, sut.AsUShort);
        }

        [Fact]
        public void UShort_address_32468_is_address_7_14_212()
        {
            var sut = new IndividualAddress(32468);
            Assert.Equal(new byte[] { 7, 14, 212 }, sut.As3Level);
        }

        [Fact]
        public void Address_with_area_18_cannot_be_constructed()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new IndividualAddress(18, 0, 0));
        }

        [Fact]
        public void Address_with_line_19_cannot_be_constructed()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new IndividualAddress(0, 19, 0));
        }
    }
}
