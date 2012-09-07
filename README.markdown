SerialServe
===========

SerialServe is a HTTP server which exposes local Serial ports to a web browser.

Once you've started the server, you can connect to SerialServe by connecting to [localhost on port 9981](http://localhost:9981/). You should see a welcome message.

Use
===

SerialServe sends CORS headers, enabling communication with Javascript running on remote sites. **(This means visiting untrusted web pages with SecureServe running is terribly insecure!)**

You can use the Web library to access the APIs. The API uses RequireJS module format.

Raw API Calls
=============

All API calls return JSON. `:port` should be an integer (e.g. for COM3, use 3).

/list
-----
Gets a list of all available COM ports.

/disable/:port
--------------
Disables the port (pulls the /Enable line low by setting DTR false in Windows).

/enable/:port
-------------
Enables the port (pulls the /Enable line high by setting DTR true in Windows).

/read/:port
-----------
Opens a request to handle incoming data. This request will block until data is recieved, or until it times out. You should always have at least one open. If more than one is open, it will send it to the longest-running connection.

Read more about this method of push notifications on [the Wikipedia article on Comet](http://en.wikipedia.org/wiki/Comet_(programming)#Ajax_with_long_polling).