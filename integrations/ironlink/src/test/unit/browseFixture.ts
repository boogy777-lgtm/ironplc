/**
 * A browse response in the shape `ironlink serve` prints (camel case; the
 * identity fields of a variant and the route fields of a seen route are
 * flattened, as the program serializes them). Four things the tree must show:
 * - "Press line 3" answers by broadcast and by the address list: one device
 *   seen by two drivers;
 * - "Mixer" and "Mixer (copy)" answer under one device id with different
 *   identities, at two addresses: an identity conflict;
 * - "Packer" is protected and is the stored route of its device id;
 * - one device answered with another discovery wire version.
 * A stored route to a device that no driver heard is in `routesNotSeen`.
 */

import { BrowseResponse, parseResponseLine } from '../../ironlinkProtocol';

/** The time the fixture was "taken", in milliseconds since the Unix epoch. */
export const FIXTURE_NOW = 1761136010000;

export const PRESS_LINE_ID = '00112233445566778899aabbccddeeff';
export const MIXER_ID = 'a0a1a2a3a4a5a6a7a8a9aaabacadaeaf';
export const PACKER_ID = '0f0e0d0c0b0a09080706050403020100';
export const SILENT_ID = 'ffeeddccbbaa99887766554433221100';

export const BROWSE_LINE = JSON.stringify({
  response: 'browse',
  workstation: { name: 'PC-ONE' },
  drivers: [
    {
      kind: 'broadcast',
      targets: ['192.168.1.20 -> 255.255.255.255:49152'],
      devices: [
        {
          deviceId: MIXER_ID,
          conflict: true,
          variants: [
            {
              name: 'Mixer',
              model: 'IronPLC SoftPLC',
              runtimeVersion: '0.247.0',
              protocol: 1,
              protected: false,
              routes: [{
                driver: 'broadcast',
                address: '192.168.1.71',
                sessionPort: 49152,
                method: 'tcp',
                seenAt: 1761136008000,
                routed: false,
              }],
            },
            {
              name: 'Mixer (copy)',
              model: 'IronPLC SoftPLC',
              runtimeVersion: '0.246.0',
              protocol: 1,
              protected: false,
              routes: [{
                driver: 'broadcast',
                address: '192.168.1.72',
                sessionPort: 49152,
                method: 'tcp',
                seenAt: 1761136007500,
                routed: false,
              }],
            },
          ],
        },
        {
          deviceId: PACKER_ID,
          conflict: false,
          variants: [{
            name: 'Packer',
            model: 'IronPLC SoftPLC',
            runtimeVersion: '0.247.0',
            protocol: 1,
            protected: true,
            routes: [{
              driver: 'broadcast',
              address: '192.168.1.60',
              sessionPort: 49152,
              method: 'tcp',
              seenAt: 1761136009000,
              routed: true,
            }],
          }],
        },
        {
          deviceId: PRESS_LINE_ID,
          conflict: false,
          variants: [{
            name: 'Press line 3',
            model: 'IronPLC SoftPLC',
            runtimeVersion: '0.247.0',
            protocol: 1,
            protected: false,
            routes: [{
              driver: 'broadcast',
              address: '192.168.1.50',
              sessionPort: 49152,
              method: 'tcp',
              seenAt: 1761136000123,
              routed: false,
            }],
          }],
        },
      ],
      otherVersions: [{ address: '192.168.1.90', wireVersion: 2, seenAt: 1761136005000 }],
      undecodable: 1,
    },
    {
      kind: 'addressList',
      targets: ['10.0.4.7:49152', '192.168.1.50:49152'],
      devices: [
        {
          deviceId: PRESS_LINE_ID,
          conflict: false,
          variants: [{
            name: 'Press line 3',
            model: 'IronPLC SoftPLC',
            runtimeVersion: '0.247.0',
            protocol: 1,
            protected: false,
            routes: [{
              driver: 'addressList',
              address: '192.168.1.50',
              sessionPort: 49152,
              method: 'tcp',
              seenAt: 1761136009900,
              routed: false,
            }],
          }],
        },
      ],
      otherVersions: [],
      undecodable: 0,
    },
  ],
  routesNotSeen: [{
    deviceId: SILENT_ID,
    address: '10.0.4.7',
    driver: 'addressList',
    sessionPort: 49152,
    verifiedAt: 1761050000000,
  }],
});

/** The fixture as the client delivers it: parsed from the line. */
export function browseFixture(): BrowseResponse {
  return parseResponseLine(BROWSE_LINE) as BrowseResponse;
}

/** A browse in which the one driver heard nothing. */
export const EMPTY_BROWSE_LINE = JSON.stringify({
  response: 'browse',
  workstation: { name: 'PC-ONE' },
  drivers: [{ kind: 'broadcast', targets: ['0.0.0.0 -> 255.255.255.255:49152'], devices: [], otherVersions: [], undecodable: 0 }],
  routesNotSeen: [],
});
