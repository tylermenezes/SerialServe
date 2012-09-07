SerialServe
===========

SerialServe is an HTTP server which exposes local Serial ports to a web browser.

Once you've started the server, you can connect to SerialServe by connecting to [localhost on port 9981](http://localhost:9981/). You should see a welcome message.

SerialServe sends CORS headers, enabling communication with Javascript running on remote sites. **(This means visiting untrusted web pages with SecureServe running is terribly insecure!)**

Raw API Calls
-------------

All API calls return JSON. `:port` should be an integer (e.g. for COM3, use 3).

**/list**

Gets a list of all available COM ports.

**/disable/:port**

Disables the port (pulls the /Enable line low by setting DTR false in Windows).

**/enable/:port**

Enables the port (pulls the /Enable line high by setting DTR true in Windows).

**/read/:port**

Opens a request to handle incoming data. This request will block until data is received, or until it times out. You should always have at least one open. If more than one is open, it will send it to the longest-running connection.

Read more about this method of push notifications on [the Wikipedia article on Comet](http://en.wikipedia.org/wiki/Comet_(programming)).

serial-bus.js
===

serial-bus.js is SerialServe's Javascript communication library. It exposes the following classes:

SerialBus
---------

Static class. Exposes the following methods:

 * `List(onSuccessDelegate(Array), onFailureDelegate(msg))` - Lists all attached COM ports on the current computer

SerialBus.SerialPort
--------------------

Takes the following params:

  * `port` - Port number (e.g. 3 for COM3)
  * `[timeout]` - Amount to wait between fetching more lines (e.g. between RFID line reads) in ms. Default 1000ms

Exposes the following methods:

  * `Enable(onSuccessDelegate(), onFailureDelegate(msg)` - Enables the port
  * `Disable(onSuccessDelegate(), onFailureDelegate(msg)` - Disables the port
  * `OnDataReceived.register(delegate(string))` - Registers a new function to be called when data is received on the port.

Todos
=====

This was mostly written to allow RFID communication to the browser. There is more to do:

  * Implement write()
  * Allow changing of serial paramaters on connect
  * Add OnPortConnect and OnPortDisconnect