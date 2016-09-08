window.onload = function() {
  $('#searchBtn').on("click", onSearchBtn);
};

function onSearchBtn () {
	var pattern = $('#searchInput').val();

	$.ajax({
		method 		: 'GET',
		url 		: 'results',
		cache		: false,
		dataType	: 'html',
		data 	: {
			SearchPattern : pattern
		}
	})
	.done(function(data) {
		$("html").html(data);
	})
	.fail(function() {
    	alert( "error - while loading the result view" );
  	});
}
