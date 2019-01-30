﻿namespace Buttplug.Devices.Configuration
{
    public class USBProtocolConfiguration : IProtocolConfiguration
    {
        public readonly ushort VendorId;
        public readonly ushort ProductId;

        public USBProtocolConfiguration(ushort aVendorId, ushort aProductId)
        {
            VendorId = aVendorId;
            ProductId = aProductId;
        }

        internal USBProtocolConfiguration(USBIdentifier aId, USBConfiguration aConfig)
        {
            VendorId = aId.VendorId;
            ProductId = aId.ProductId;
        }

        public bool Matches(IProtocolConfiguration aConfig)
        {
            return aConfig is USBProtocolConfiguration usbConfig && usbConfig.ProductId == ProductId && usbConfig.VendorId == VendorId;
        }
    }
}