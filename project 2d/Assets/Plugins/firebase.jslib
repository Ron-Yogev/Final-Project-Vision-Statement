mergeInto(LibraryManager.Library, {

	GetJSON: function (path, objectName, callback, fallback) {
		var parsedPath = Pointer_stringify(path);
		var parsedObjectName = Pointer_stringify(objectName);
		var parsedCallback = Pointer_stringify(callback);
		var parsedFallback = Pointer_stringify(fallback);

		try {

		firebase.database().ref(parsedPath).once('value').then(function(snapshot) {
			unityInstance.Module.SendMessage(parsedObjectName, parsedCallback, JSON.stringify(snapshot.val()));
		});

		} catch (error){
			unityInstance.Module.SendMessage(parsedObjectName, parsedFallback, "There was an error: "+ error.message);
		}
	}
});