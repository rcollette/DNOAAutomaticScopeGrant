var utilities = {
    assembleQueryString: function (args) {
        var query = '?';
        for (var key in args) {
            if (query.length > 1) query += '&';
            query += encodeURIComponent(key) + '=' + encodeURIComponent(args[key])
        };
        return query;
    },

    parseQueryString: function (query) {
        var result = new Array();
        var pairs = query.split('&');
        for (var i = 0; i < pairs.length; i++) {
            var pair = pairs[i].split('=');
            var key = decodeURIComponent(pair[0]);
            var value = decodeURIComponent(pair[1]);
            result[key] = value;
        };
        return result;
    },

    stripQueryAndFragment: function (url) {
        var index = url.indexOf('?');
        if (index < 0) index = url.indexOf('#');
        url = index < 0 ? url : url.substring(0, index);
        return url;
    }
};

$(document).ready(function () {
    var requestAuthorizationButton = $('#requestAuthorizationButton');

    function gatherRequestedScopes() {
        scopes = '';
        var scopeElements = $("input[name='scope']");
        for (var i = 0; i < scopeElements.length; i++) {
            if (scopeElements[i].checked) {
                if (scopes.length > 0) scopes += ' ';
                scopes += scopeElements[i].value;
            }
        };
        return scopes;
    };

    function requestAuthorization() {
        var args = new Array();
        args['scope'] = '/DataApi.svc/web/UserProfile';
        var location = document.location;
        args['redirect_uri'] = utilities.stripQueryAndFragment(applicationRoot + "ClientLogOn.ashx");
        args['response_type'] = 'token';
        args['client_id'] = 'sampleImplicitConsumer';

        var authorizeUrl = "http://localhost:50172/OAuth/Authorize" + utilities.assembleQueryString(args);
        document.location = authorizeUrl;
    };
});

$(document).ready(function () {
});