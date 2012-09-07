define(['jquery'], function(jQuery){
    var Event = function()
    {
        var _delegates = [];
        /**
         * Registers an event handler
         * @param  {function(...*)} delegate   Event handler to register
         */
        this.register = function(delegate)
        {
            _delegates.push(delegate);
        }

        /**
         * Removes an event handler
         *  @param  {function(...*)} delegate   Event handler to remove
         */
        this.deregister = function(delegate)
        {
            for (var i in _delegates) {
                if (_delegates[i] == delegate) {
                    _delegates.splice(i, 1);
                }
            }
        }

        /**
         * Calls a function, passing in an array of values as position-wise arguments
         * e.g. callUserFuncArray(lambda, [1, 2, 3, 'a', 'b', 'c']) calls lambda(1, 2, 3, 'a', 'b', 'c');
         * @param  {function(...*)} delegate   Function to execute
         * @param  {Array.<*>}      parameters Paramaters to pass to the function
         * @return {*}              Result of the function
         */
        var callUserFuncArray = function (delegate, parameters) {
            var func;

            if (typeof delegate === 'string') {
                func = (typeof this[delegate] === 'function') ? this[delegate] : func = (new Function(null, 'return ' + delegate))();
            }
            else if (Object.prototype.toString.call(delegate) === '[object Array]') {
                func = (typeof delegate[0] == 'string') ? eval(delegate[0] + "['" + delegate[1] + "']") : func = delegate[0][delegate[1]];
            }
            else if (typeof delegate === 'function') {
                func = delegate;
            }

            if (typeof func !== 'function') {
                throw new Error(func + ' is not a valid function');
            }

            return (typeof delegate[0] === 'string') ? func.apply(eval(delegate[0]), parameters) : (typeof delegate[0] !== 'object') ? func.apply(null, parameters) : func.apply(delegate[0], parameters);
        }

        /**
         * Executes all the registered event handlers
         * @return {...*} Paramaters to pass to the handlers
         */
        this.apply = function()
        {
            for (var i in _delegates) {
                callUserFuncArray(_delegates[i], arguments);
            }
        }
    }

    var endpoint = "http://127.0.0.1:9981/";

    /**
     * Represents a serial port
     * @param {number} port    The port number to connect to (e.g. 3 for COM3)
     * @param {number} timeout Timeout between successful reads
     */
    var SerialPort = function(port, timeout)
    {
        var _this = this;

        /**
         * Event handler which dispatches events when data is recieved.
         * @type {Event}
         */
        this.OnDataReceived = new Event();

        /**
         * Disables the port.
         * @param {function(*)}             onDisabledDelegate Function to execute when enabled successfully
         * @param {function(string,string)} onFailureDelegate  Function to execute when the request fails
         */
        this.Disable = function(onDisabledDelegate, onFailureDelegate)
        {
            jQuery.ajax({
                url: endpoint + 'disable/' + port,
                success: onDisabledDelegate,
                error: onFailureDelegate,
                dataType: 'json'
            });
        }

        /**
         * Enables the port.
         * @param {function(*)}             onEnabledDelegate  Function to execute when enabled successfully
         * @param {function(string,string)} onFailureDelegate  Function to execute when the request fails
         */
        this.Enable = function(onEnabledDelegate, onFailureDelegate)
        {
            jQuery.ajax({
                url: endpoint + 'enable/' + port,
                success: onEnabledDelegate,
                error: onFailureDelegate,
                dataType: 'json'
            });
        }

        /**
         * Executed when data is recieved from the port, dispatched the event handler and restarts a request
         * @param  {*}      data Data recieved
         */
        var dataRecieved = function(data)
        {
            if (typeof(data.response) !== 'undefined') {
                _this.OnDataReceived.apply(data.response);
            }

            setTimeout(startLongPollThread, timeout);
        }

        /**
         * Starts a new long poll thread
         */
        var startLongPollThread = function()
        {
            jQuery.ajax({
                url: endpoint + 'read/' + port,
                success: dataRecieved,
                error: function(err,e)
                {
                    setTimeout(startLongPollThread, timeout);
                },
                dataType: 'json'
            });
        }

        /**
         * Initializes the port connection
         */
        this.constructor = function()
        {
            if (typeof(timeout) === 'undefined') {
                timeout = 1000;
            }
            startLongPollThread();
        }
        this.constructor();
    }

    var SerialBus = function()
    {
        /**
         * SerialPort class
         */
        this.SerialPort = SerialPort;

        /**
         * Gets a list of all attached ports.
         * @param {function(Array.<number>)}             onSuccessDelegate  Function to execute when the list is received, taking a list of COM port
         *                                                                  numbers which are attached
         * @param {function(string,string)}              onFailureDelegate  Function to execute when the request fails
         */
        this.List = function(onSuccessDelegate, onFailureDelegate)
        {
            jQuery.ajax({
                url: endpoint + 'list/',
                success: onSuccessDelegate,
                error: onFailureDelegate,
                dataType: 'json'
            });
        }
    }

    return (new SerialBus());
});
