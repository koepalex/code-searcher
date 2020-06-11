window.onload = function() {
  $('#searchBtn').on("click", onSearchBtn);
};

function onSearchBtn () {
	var pattern = $('#searchInput').val();
	var numberOfHits = $('#searchHits').val();

	$.ajax({
		method 		: 'GET',
		url 		: 'results',
		cache		: false,
		dataType	: 'html',
		data 	: {
			SearchPattern : pattern,
			MaximumNumberOfHits : numberOfHits
		}
	})
	.done(function(data) {
		//$("html").html(data);
		document.write(data);
	})
	.fail(function() {
    	alert( "error - while loading the result view" );
  	});
}
