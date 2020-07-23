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
		document.write(data); //lgtm [js/eval-like-call]
	})
	.fail(function() {
    	alert( "error - while loading the result view" );
  	});
}
