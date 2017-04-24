using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using KnxNetCore.Telegrams;

namespace KnxNetCore
{
    internal class Serializer
    {
        // TODO: accept cEMI protocol only.
        // TODO: Do not accept extended frames.

        private static readonly Dictionary<ushort, Func<KnxTelegramHeader, byte[], KnxTelegramPayload>> PayloadFactories =
            new Dictionary<ushort, Func<KnxTelegramHeader, byte[], KnxTelegramPayload>>
            {
                {0x206, ParseConnectResponse},
                {0x208, ParseConnectionStateResponse},
                {0x209, ParseDisconnectRequest},
                {0x420, ParseTunnelingRequest},
                {0x421, ParseTunnelingAcknowledge}
            };

        public static void Serialize(KnxTelegramHeader knxTelegramHeader, MemoryStream stream)
        {
            stream.WriteByte(knxTelegramHeader.HeaderLength);
            stream.WriteByte(knxTelegramHeader.ProtocolVersion);
            stream.WriteByte((byte)(knxTelegramHeader.ServiceType >> 8));
            stream.WriteByte((byte)(knxTelegramHeader.ServiceType & 255));
            stream.WriteByte((byte)(knxTelegramHeader.TotalLength >> 8));
            stream.WriteByte((byte)(knxTelegramHeader.TotalLength & 255));
        }

        public static void Serialize(KnxConnectRequest knxConnectRequest, MemoryStream stream)
        {
            stream.SetLength(0);
            stream.Seek(6, SeekOrigin.Begin);
            Serialize(knxConnectRequest.IpEndPoint, stream);
            Serialize(knxConnectRequest.IpEndPoint, stream);
            stream.WriteByte(4);
            stream.WriteByte(4);
            stream.WriteByte(2);
            stream.WriteByte(0);
            stream.Seek(0, SeekOrigin.Begin);
            var header = new KnxTelegramHeader(6, 0x10, 0x205, (ushort)stream.Length);
            Serialize(header, stream);
        }

        public static void Serialize(KnxConnectionStateRequest knxConnectionStateRequest, MemoryStream stream)
        {
            stream.SetLength(0);
            stream.Seek(6, SeekOrigin.Begin);
            stream.WriteByte(knxConnectionStateRequest.ChannelId);
            stream.WriteByte(0);
            Serialize(knxConnectionStateRequest.EndPoint, stream);
            stream.Seek(0, SeekOrigin.Begin);
            var header = new KnxTelegramHeader(6, 0x10, 0x207, (ushort)stream.Length);
            Serialize(header, stream);
        }

        public static void Serialize(KnxTunnelingRequest knxTunnelingRequest, MemoryStream memoryStream)
        {
            memoryStream.SetLength(0);
            memoryStream.Seek(6, SeekOrigin.Begin);
            SerializeCommonConnectionHeader(knxTunnelingRequest.ChannelId, knxTunnelingRequest.SequenceNumber, memoryStream);
            Serialize(knxTunnelingRequest.CemiFrame, memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var header = new KnxTelegramHeader(6, 0x10, 0x420, (ushort)memoryStream.Length);
            Serialize(header, memoryStream);
        }

        private static void SerializeCommonConnectionHeader(byte channelId, byte sequenceNumber,
            MemoryStream memoryStream)
        {
            memoryStream.WriteByte(4); // Length
            memoryStream.WriteByte(channelId);
            memoryStream.WriteByte(sequenceNumber);
            memoryStream.WriteByte(0); // Reserved            
        }

        public static void Serialize(KnxTunnelingAcknowledge knxTunnelingAcknowledge, MemoryStream memoryStream)
        {
            memoryStream.SetLength(0);
            memoryStream.Seek(6, SeekOrigin.Begin);
            SerializeCommonConnectionHeader(knxTunnelingAcknowledge.ChannelId, knxTunnelingAcknowledge.SequenceNumber, memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var header = new KnxTelegramHeader(6, 0x10, 0x421, (ushort)memoryStream.Length);
            Serialize(header, memoryStream);
        }

        public static void Serialize(CemiFrame cemiFrame, MemoryStream memoryStream)
        {
            memoryStream.WriteByte(cemiFrame.MessageCode);
            memoryStream.WriteByte(0); // No additional data
            memoryStream.WriteByte((byte)cemiFrame.Control1);
            memoryStream.WriteByte((byte)cemiFrame.Control2);
            memoryStream.WriteByte(0); // Source Address will be filled out by gateway
            memoryStream.WriteByte(0);
            var destinationAddressAsUShort = cemiFrame.DestinationAddress.AsUShort;
            memoryStream.WriteByte((byte)(destinationAddressAsUShort >> 8));
            memoryStream.WriteByte((byte)(destinationAddressAsUShort & 255));
            memoryStream.WriteByte(cemiFrame.DataLength);
            memoryStream.WriteByte((byte)(cemiFrame.Apdu >> 8));
            memoryStream.WriteByte((byte)cemiFrame.Apdu);
            if (cemiFrame.Data.Count != 0)
            {
                memoryStream.Write(cemiFrame.Data.Array, cemiFrame.Data.Offset, cemiFrame.Data.Count);
            }
        }

        public static void Serialize(IPEndPoint ipEndPoint, MemoryStream stream)
        {
            // Serialize "Host Protocol Address Information" (HPAI)
            if (ipEndPoint.Address.GetAddressBytes().Length != 4)
            {
                throw new ArgumentException(string.Format("Cannot serialize IP Address {0}", ipEndPoint.Address));
            }
            stream.WriteByte(8); // Length
            stream.WriteByte(1); // Protocol Type
            stream.Write(ipEndPoint.Address.GetAddressBytes(), 0, 4);
            stream.WriteByte((byte)(ipEndPoint.Port >> 8));
            stream.WriteByte((byte)ipEndPoint.Port);
        }

        private static ushort ToUInt16(byte[] buffer, int startIndex)
        {
            return (ushort)((buffer[startIndex] << 8) + buffer[startIndex + 1]);
        }

        internal static KnxTelegramHeader ParseKnxTelegramHeader(byte[] buffer)
        {
            if (buffer.Length < 6)
            {
                throw new ArgumentException(string.Format("Supplied buffer smaller than required header size ({0})", buffer.Length));
            }
            var knxTelegram = new KnxTelegramHeader(buffer[0], buffer[1], ToUInt16(buffer, 2), ToUInt16(buffer, 4));
            if (knxTelegram.HeaderLength != 6)
            {
                throw new ArgumentException(string.Format("Header length is not 6 but {0}", knxTelegram.HeaderLength));
            }
            if (knxTelegram.ProtocolVersion != 0x10)
            {
                throw new ArgumentException(string.Format("ProtocolVersion is not 0x10 but {0}", knxTelegram.ProtocolVersion));
            }
            return knxTelegram;
        }

        internal static KnxTelegramPayload ParseKnxTelegram(KnxTelegramHeader header, byte[] buffer)
        {
            if (!PayloadFactories.ContainsKey(header.ServiceType))
            {
                throw new ArgumentException(string.Format("Do not know how to deserialize service type:  {0}", header.ServiceType));
            }
            return PayloadFactories[header.ServiceType](header, buffer);
        }

        private static KnxTelegramPayload ParseDisconnectRequest(KnxTelegramHeader arg1, byte[] arg2)
        {
            if (arg2.Length < 8)
            {
                throw new ArgumentException(string.Format("Supplied buffer smaller than required size ({0})", arg2.Length));
            }
            return new KnxDisconnectRequest(arg2[6], arg2[7]);
        }

        private static KnxTelegramPayload ParseTunnelingRequest(KnxTelegramHeader arg1, byte[] arg2)
        {
            var dataLength = arg2[18] & 15;
            var cemiFrame = new CemiFrame(
                arg2[10],
                (CemiFrame.Control1Flags)arg2[12],
                (CemiFrame.Control2Flags)arg2[13],
                IndividualAddress.FromUShort((ushort)((arg2[14] << 8) + arg2[15])),
                GroupAddress.FromUShort((ushort)((arg2[16] << 8) + arg2[17])),
                arg2[18],
                (ushort)((arg2[19] << 8) + arg2[20]), new ArraySegment<byte>(arg2, 21, dataLength == 0 ? 0 : dataLength - 1)); // protect against unknown datagrams with datalength = 0
            return new KnxTunnelingRequest(arg2[7], arg2[8], cemiFrame);
        }

        private static KnxTelegramPayload ParseTunnelingAcknowledge(KnxTelegramHeader arg1, byte[] arg2)
        {
            if (arg2.Length < 8)
            {
                throw new ArgumentException(string.Format("Supplied buffer smaller than required size ({0})", arg2.Length));
            }
            return new KnxTunnelingAcknowledge(arg2[7], arg2[8]);
        }

        private static KnxTelegramPayload ParseConnectionStateResponse(KnxTelegramHeader arg1, byte[] arg2)
        {
            if (arg2.Length < 8)
            {
                throw new ArgumentException(string.Format("Supplied buffer smaller than required size ({0})", arg2.Length));
            }
            return new KnxConnectionStateResponse(arg2[6], (KnxConnectionStateResponse.StatusCodes)arg2[7]);
        }

        private static KnxTelegramPayload ParseConnectResponse(KnxTelegramHeader arg1, byte[] arg2)
        {
            if (arg2.Length < 8)
            {
                throw new ArgumentException(string.Format("Supplied buffer smaller than required size ({0})", arg2.Length));
            }
            return new KnxConnectResponse(arg2[6], (KnxConnectResponse.StatusCodes)arg2[7]);
        }
    }
}