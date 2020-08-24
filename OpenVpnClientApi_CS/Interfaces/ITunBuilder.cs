﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVpnClientApi_CS.Interfaces
{
    public interface ITunBuilder
    {
        // Tun builder methods.
        // Methods documented in openvpn/tun/builder/base.hpp
        bool TunBuilderSetRemoteAddress(string address, bool ipv6);
        bool TunBuilderAddAddress(string address, int prefix_length, string gateway, bool ipv6, bool net30);
        bool TunBuilderRerouteGw(bool ipv4, bool ipv6, long flags);
        bool TunBuilderAddRoute(string address, int prefix_length, bool ipv6);
        bool TunBuilderExcludeRoute(string address, int prefix_length, bool ipv6);
        bool TunBuilderAddDnsServer(string address, bool ipv6);
        bool TunBuilderAddSearchDomain(string domain);
        bool TunBuilderSetMtu(int mtu);
        bool TunBuilderSetSessionName(string name);
        int TunBuilderEstablish();
        void TunBuilderTeardown(bool disconnect);
    }
}
