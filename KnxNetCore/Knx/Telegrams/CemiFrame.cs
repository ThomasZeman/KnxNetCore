using System;

namespace KnxNetCore.Knx.Telegrams
{
    public class CemiFrame // common External Message Interface
    {
        public enum Commands : byte
        {
            ValueRead = 0,
            ValueResponse = 1,
            ValueWrite = 2,
            Unknown3 = 3,
            Unknown4 = 4,
            Unknown5 = 5,
            Unknown6 = 6,
            Unknown7 = 7,
            Unknown8 = 8,
            Unknown9 = 9,
            MemoryWrite = 10,
            Unknown11 = 11,
            Unknown12 = 12,
            Unknown13 = 13,
            Unknown14 = 14,
            Unknown15 = 15
        }

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
            GroupAddress = 0x80,
            HopCount2 = 0x40,
            HopCount1 = 0x20,
            HopCount0 = 0x10
        }
        //
        // transport layer control information (TPCI)
        // application layer control information (APCI)
        // application control bits (APCI)
        // application protocol data unit (APDU) 
        //
        //
        //

        //+--------+--------+--------+--------+---------+---------+--------+---------+
        //|  Msg   |Add.Info| Ctrl 1 | Ctrl 2 | Source  | Dest.   |  Data  |   APDU  |
        //| Code   | Length |        |        | Address | Address | Length |         |
        //+--------+--------+--------+--------+---------+---------+--------+---------+
        //  1 byte   1 byte   1 byte   1 byte   2 bytes   2 bytes   1 byte   2 bytes

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

        //Data Link Layer Message

        //Primitive Code
        //---------------  -------

        //L_Raw.req          0x10
        //L_Data.req         0x11  Data Service.Primitive used for transmitting a data frame
        //L_Poll_Data.req    0x13  Poll Data Service

        //    FROM DATA LINK LAYER TO NETWORK LAYER

        //Data Link Layer Message

        //Primitive Code
        //---------------  -------

        //L_Poll_Data.con    0x25  Poll Data Service
        //    L_Data.ind     0x29  Data Service. Primitive used for receiving a data frame

        //L_Busmon.ind       0x2B  Bus Monitor Service
        //    L_Raw.ind      0x2D

        //L_Data.con         0x2E  Data Service. Primitive used for local confirmation that a frame was sent

        //    (does not indicate a successful receive though)

        //L_Raw.con          0x2F

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

        public enum MessageCodes : byte
        {
            DataRequest = 0x11,
            DataIndication = 0x29,
            DataConfirmation = 0x2e,

            RawRequest = 0x10,
            RawIndication = 0x2d,
            RawConfirmation = 0x2f,

            PollDataRequest = 0x13,
            PollDataConfirmation = 0x25,
            BusmonitorIndication = 0x2b
        }

        public CemiFrame(MessageCodes messageCode, Control1Flags control1, Control2Flags control2, IndividualAddress sourceAddress, GroupAddress destinationAddress,
            byte dataLength, ushort apdu)
            : this(messageCode, control1, control2, sourceAddress, destinationAddress, dataLength, apdu, new ArraySegment<byte>())
        {
        }

        public CemiFrame(MessageCodes messageCode, Control1Flags control1, Control2Flags control2, IndividualAddress sourceAddress, GroupAddress destinationAddress,
            byte dataLength, ushort apdu, ArraySegment<byte> data)
        {
            MessageCode = messageCode;
            Control1 = control1;
            Control2 = control2;
            SourceAddress = sourceAddress;
            DestinationAddress = destinationAddress;
            DataLength = dataLength;
            Apdu = apdu;
            Data = data;
        }

        public MessageCodes MessageCode { get; }

        public Control1Flags Control1 { get; }

        public Control2Flags Control2 { get; }

        public Commands Command => (Commands) ((Apdu >> 6) & 15);

        public IndividualAddress SourceAddress { get; }
        public GroupAddress DestinationAddress { get; }
        public byte DataLength { get; }
        public ushort Apdu { get; }
        public ArraySegment<byte> Data { get; }

        public override string ToString()
        {
            return
                $"MessageCode: {MessageCode}, Control1: {Control1}, Control2: {Control2}, SourceAddress: {SourceAddress}, DestinationAddress: {DestinationAddress}, DataLength: {DataLength}, Apdu: {Apdu}";
        }
    }
}