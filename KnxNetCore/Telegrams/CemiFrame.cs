using System;

namespace KnxNetCore.Telegrams
{
    public class CemiFrame
    {
        //+---------+--------+--------+--------+--------+---------+---------+--------+---------+
        //| Header  |  Msg   |Add.Info| Ctrl 1 | Ctrl 2 | Source  | Dest.   |  Data  |   APDU  |
        //|         | Code   | Length |        |        | Address | Address | Length |         |
        //+---------+--------+--------+--------+--------+---------+---------+--------+---------+
        //  6 bytes   1 byte   1 byte   1 byte   1 byte   2 bytes   2 bytes   1 byte   2 bytes

        //    Header = See below the structure of a cEMI header
        //    Message Code    = See below.On Appendix A is the list of all existing EMI and cEMI codes
        //    Add.Info Length = 0x00 - no additional info

        //    Control Field 1 =
        //    Control Field 2 =

        //    Source Address   = 0x0000 - filled in by router/gateway with its source address which is
        //                          part of the KNX subnet

        //    Dest. Address    = KNX group or individual address (2 byte)
        //    Data Length      = Number of bytes of data in the APDU excluding the TPCI/APCI bits
        //    APDU             = Application Protocol Data Unit - the actual payload including transport
        //                       protocol control information(TPCI), application protocol control
        //                       information(APCI) and data passed as an argument from higher layers of
        //                       the KNX communication stack

        //Control Field 1

        // Bit  |
        //------+---------------------------------------------------------------
        //  7   | Frame Type  - 0x0 for extended frame
        //      |               0x1 for standard frame
        //------+---------------------------------------------------------------
        //  6   | Reserved
        //      |
        //------+---------------------------------------------------------------
        //  5   | Repeat Flag - 0x0 repeat frame on medium in case of an error
        //      |               0x1 do not repeat
        //------+---------------------------------------------------------------
        //  4   | System Broadcast - 0x0 system broadcast
        //      |                    0x1 broadcast
        //------+---------------------------------------------------------------
        //  3   | Priority    - 0x0 system
        //      |               0x1 normal
        //------+               0x2 urgent
        //  2   |               0x3 low
        //      |
        //------+---------------------------------------------------------------
        //  1   | Acknowledge Request - 0x0 no ACK requested
        //      | (L_Data.req)          0x1 ACK requested
        //------+---------------------------------------------------------------
        //  0   | Confirm      - 0x0 no error
        //      | (L_Data.con) - 0x1 error
        //------+---------------------------------------------------------------

        //Control Field 2

        // Bit  |
        //------+---------------------------------------------------------------
        //  7   | Destination Address Type - 0x0 individual address
        //      |                          - 0x1 group address
        //------+---------------------------------------------------------------
        // 6-4  | Hop Count(0-7)
        //------+---------------------------------------------------------------
        // 3-0  | Extended Frame Format - 0x0 standard frame
        //------+---------------------------------------------------------------

        [Flags]
        public enum Control1Flags : byte
        {
            Error = 0x1,
            AckRequested = 0x2,
            PrioritySystem = 0x0,
            PriorityNormal = 0x4,
            PriorityUrgent = 0x8,
            PriorityLow = 0x0c,
            Broadcast = 0x10,
            DoNotRepeat = 0x20,
            StandardFrame = 0x80
        }

        public enum Control2Flags : byte
        {
            GroupAddress = 0x80
        }

        public CemiFrame(byte messageCode, Control1Flags control1, Control2Flags control2, ushort sourceAddress,
            ushort destinationAddress, byte dataLength, ushort apdu)
        {
            MessageCode = messageCode;
            Control1 = control1;
            Control2 = control2;
            SourceAddress = IndividualAddress.FromUShort( sourceAddress);
            DestinationAddress = GroupAddress.FromUShort(destinationAddress);
            DataLength = dataLength;
            Apdu = apdu;
        }

        public byte MessageCode { get; }

        public Control1Flags Control1 { get; }

        public Control2Flags Control2 { get; }

        public IndividualAddress SourceAddress { get; }
        public GroupAddress DestinationAddress { get; }
        public byte DataLength { get; }
        public ushort Apdu { get; }

        public override string ToString()
        {
            return
                $"MessageCode: {MessageCode}, Control1: {Control1}, Control2: {Control2}, SourceAddress: {SourceAddress}, DestinationAddress: {DestinationAddress}, DataLength: {DataLength}, Apdu: {Apdu}";
        }
    }
}