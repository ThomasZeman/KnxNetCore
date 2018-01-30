using System;
using KnxNetCore.Telegrams;
using Xunit;

namespace KnxNetCore.UnitTests.Telegrams
{
    public class GroupAddressTests
    {
        [Fact]
        public void UShort_address_775_is_0_3_7()
        {
            var sut = GroupAddress.FromUShort(775);
            var result = sut.As3Level;
            Assert.Equal(new byte[] { 0, 3, 7 }, result);
        }

        [Fact]
        public void Address_0_3_7_is_ushort_address_775()
        {
            var sut = GroupAddress.FromGroups(0, 3, 7);
            Assert.Equal(775, sut.AsUShort);
        }

        [Fact]
        public void UShort_address_40577_is_12_6_129()
        {
            var sut = GroupAddress.FromUShort(40577);
            var result = sut.As3Level;
            Assert.Equal(new byte[] { 19, 6, 129 }, result);
        }

        [Fact]
        public void Address_12_6_129_is_ushort_address_40577()
        {
            var sut = GroupAddress.FromGroups(19, 6, 129);
            Assert.Equal(40577, sut.AsUShort);
        }

        [Fact]
        public void Address_with_main_group_33_cannot_be_constructed()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => GroupAddress.FromGroups(33, 0, 0));
        }

        [Fact]
        public void Address_with_middle_group_10_cannot_be_constructed()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => GroupAddress.FromGroups(0, 10, 0));
        }

        [Fact]
        public void Two_instances_of_same_address_are_equal()
        {
            Assert.Equal(GroupAddress.FromGroups(10, 5, 4), GroupAddress.FromGroups(10, 5, 4));
        }

        [Fact]
        public void Two_instances_of_different_addresses_are_not_equal()
        {
            Assert.NotEqual(GroupAddress.FromGroups(9, 5, 4), GroupAddress.FromGroups(10, 5, 4));
        }

        [Fact]
        public void ToString_prints_ets_like_group_address()
        {
            Assert.Equal("5/4/9", GroupAddress.FromGroups(5, 4, 9).ToString());
        }
    }
}
