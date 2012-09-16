//requires jQuery
//usage
//		$.ajaxReq({[options]});
//
//		options are as follows...




(function($) {

	$.AjaxRequest = function (options) {
		if ($.isFunction(this)) {
			var $self = $.extend(new $.AjaxRequest, {});
			$self.init(options);
		}
	}

	$.extend($.AjaxRequest.prototype, {
		defaults: 		{
			//events to fire depending on ajax result
			OnSuccess: 	function(response, aspRequest) { console.log(response); console.log(aspRequest); },
			OnError:		function(message, exception)   { console.log(message); console.log(exception); },
			
			//ajax veriables
			type: 						'POST',
			contentType: 				'application/json; charset=utf8',
			dataType: 					'json',
			url: 						'/ajax/AjaxListener.asmx/HelloWorld',
			ClassAndMethod: 			'testhandler.testcall',
			
			//ui variables
			pauseWebUI: 				false,
			disableWorkingIndicator: 	false,
			overrideData: 				null,
			
			//will be used when authentication is used...
			authenticationUrl: 			'',
			authenticationHandler: 		'',
			authenticationMethod: 		'',

			parameters:					null
		},
		options:		null,
		init: 			function(options){
			var self = this;
			
			//init code goes here...
			self.options = $.extend({}, self.defaults, options);

			self.data = {
				request: {
					ClassAndMethod: self.options.ClassAndMethod,
					Parameters: self.options.parameters
				}
			};

			$.ajax({
				type: self.options.type,
				url: self.options.url,
				contentType: self.options.contentType,
				dataType: self.options.dataType,
				data: JSON.stringify(self.data),
				beforeSend: function () {
					if (self.options.disableIndicator)
						return;
				
				/*
					if (self.options.blocking)
						shade(self.options.method.replace('.', ''), true);
					else
						loading(self.options.method.replace('.', ''));
				*/
				},
				complete: function () {
					if (self.options.disableIndicator)
						return;

				/*
					if (self.options.blocking)
						shade(self.options.method.replace('.', ''), true).remove();
					else
						loading(self.options.method.replace('.', '')).slideUp(100, function () { $(this).remove(); });
				*/
				},
				success: function (response) {
					if (response.d != undefined) {
						var respObject = JSON.parse(response.d);

						switch (respObject.Message) {
							case 'ok':
								self.options.OnSuccess.apply(this, [respObject.Data, respObject.Request]);
								break;

							case 'waiting':
								self.options.OnSuccess.apply(this, [respObject]);
								break;

							case 'not authenticated':
								/*

								login(function (success) {
									if (success)
										$.AjaxHandler(options);
									else
										window.location = '/Login.aspx';
								});

								*/
								break;

							case 'not allowed':
								self.options.OnError.apply(this, ['You do not have permission to perform this action.']);
								break;

							default:
								self.options.OnError.apply(this, [respObject.Message, respObject.Data]);
								break;
						}
					}
					else
						self.options.OnError.apply(this, ["Response from Ajax callback did not have any data (web server is probably down)."]);
				},
				error: function (request, status, error) {
					self.options.OnError.apply(this, [request.status + ': ' + request.statusText]);
				}
			});
		},
	});
	
})(jQuery);










/*			original from mx
(function ($) {

	$.AjaxHandler = function (options) {
		if ($.isFunction(this)) {
			var method = $.extend(new $.AjaxHandler, {});
			method.fire(options);
		}

	}

	$.extend($.AjaxHandler.prototype, {
		defaults: {
			type: 'POST',
			contentType: 'application/json; charset=utf8',
			dataType: 'json',
			events: {
				Success: function (response, aspRequest) { },
				Error: function (message, exception) { error(message, "Request String:  " + this.data); error(null, exception); }
			},
			blocking: false,
			disableIndicator: false,
			overrideData: null,
			url: '/handlers/AjaxHandler.asmx/AjaxInvocation',
			method: 'DefaultHandler.TestAsyncCalls',
			authUrl: '',
			authHandler: '',
			authMethod: '',
			parameters: {}
		},
		data: null,
		options: null,
		fire: function (options) {
			var self = this;
			self.options = $.extend(true, {}, self.defaults, options);

			self.data = {
				request: {
					Method: self.options.method,
					Parameters: self.options.parameters
				}
			};

			if (self.options.overrideData != null)
				self.data.request.Parameters = self.options.overrideData;

			$.ajax({
				type: self.options.type,
				url: self.options.url,
				contentType: self.options.contentType,
				dataType: self.options.dataType,
				data: JSON.stringify(self.data),
				beforeSend: function () {
					if (self.options.disableIndicator)
						return;

					if (self.options.blocking)
						shade(self.options.method.replace('.', ''), true);
					else
						loading(self.options.method.replace('.', ''));
				},
				complete: function () {
					if (self.options.disableIndicator)
						return;

					if (self.options.blocking)
						shade(self.options.method.replace('.', ''), true).remove();
					else
						loading(self.options.method.replace('.', '')).slideUp(100, function () { $(this).remove(); });
				},
				success: function (response) {
					if (response.d != undefined) {
						var respObject = JSON.parse(response.d);

						switch (respObject.Message) {
							case 'ok':
								self.options.events.Success.apply(this, [respObject.Data, respObject.Request]);
								break;

							case 'waiting':
								self.options.events.Success.apply(this, [respObject]);
								break;

							case 'not authenticated':
								login(function (success) {
									if (success)
										$.AjaxHandler(options);
									else
										window.location = '/Login.aspx';
								});
								break;

							case 'not allowed':
								self.options.events.Error.apply(this, ['You do not have permission to perform this action.']);
								break;

							default:
								self.options.events.Error.apply(this, [respObject.Message, respObject.Data]);
								break;
						}
					}
					else
						self.options.events.Error.apply(this, ["Response from Ajax callback did not have any data (web server is probably down)."]);
				},
				error: function (request, status, error) {
					self.options.events.Error.apply(this, [request.status + ': ' + request.statusText]);
				}
			});
		}
	});
})(jQuery);





//example call
$.AjaxHandler({
				method: 	'AssetRepository.GetLock',
				parameters: { AssetId: self.assetId },
				disableIndicator: 	true,
				events: 	{
					Success: 	function(response, request) {
						self.lock = response;
						self.interval = setInterval(function(){ self.refreshLock(); }, 30000);
						
						self.container.trigger('qaEye.Open');
					},
					Error: function(message) {
						error("There was a problem when trying to Lock the asset.  Please wait a moment and try again.", message + "\r\nRequest String:  " + this.data);
						self.container.trigger('qaEye.Close', false);
					}
				}
			});
			
//*/