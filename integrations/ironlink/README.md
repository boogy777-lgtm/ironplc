# IronLink

IronLink finds IronPLC controllers on your network and shows the route to each.
It is the VS Code part of the `ironlink` program and works on its own: you do not
need the IronPLC extension to see the controllers.

## Capabilities

* **Network view** — the activity bar shows the workstation, the ways of finding
  controllers (broadcast, address list) and, under each, one row for each route to
  a controller. A row shows the address, a short device id and the markers `route`
  (the route this workstation stored), `protected` and `conflict` (several devices
  answer under one device id), with the age of the observation. The tooltip shows
  the full observation.
* **Search network** — sends a fresh search and shows what answered. A stored
  route to a controller that did not answer is listed under **Routes not seen**.
* **Address list** — **Add address…** and **Remove address** keep the controllers
  that a broadcast cannot reach (behind a router) in the list that every search
  asks.
* **Identify** — asks one route again and shows the reply with its time.
* **Copy device id** — copies the 32-digit id of a controller.
* **Choose** — stores the route to the controller on this PC (the program checks
  that the controller answers with its device id), then hands the controller to the
  IronPLC extension, which remembers the connection path of a project. Choose is
  available when the IronPLC extension provides the command; otherwise the
  extension says why. When the route cannot be stored, the error is shown and
  nothing is handed over.
* **API for other extensions** — the extension returns an API when it activates:
  the stored routes, the last search, a new search, setting a route and showing the
  Network view. The IronPLC extension uses it to choose a controller.

IronLink only finds controllers. It opens no listening port, holds no session and
never writes to a controller or to a project.

## Requirements

The `ironlink` program from the IronPLC tools. The extension looks for it in the
directory of the `ironlink.path` setting, then in the `IRONPLC` environment
variable, the install directories and the directories of `PATH`.

## Settings

| Setting | Meaning |
|---|---|
| `ironlink.path` | The directory that contains the `ironlink` program. Empty means discover it. |
| `ironlink.browse.timeoutMs` | How long a search waits for answers, from 1 to 60000 milliseconds. Empty uses the default of the program. |
| `ironlink.browse.attempts` | How many times a search sends its request, from 1 to 10. Empty uses the default of the program. |

## Learn More

* [Documentation](https://www.ironplc.com/reference/ironlink/vscode-extension.html)
* [Source](https://github.com/ironplc/ironplc)

## Trademarks

IronPLC is an independent open-source project. It is not affiliated with,
endorsed by, or sponsored by any third party. All trademarks are the property of
their respective owners.
