$(document).on('click', '.panel-heading span.clickable', function(e){
    var $this = $(this);
	if(!$this.hasClass('panel-collapsed')) {
		$this.parents(".result-group").find('.panel-body').slideUp();
		$this.addClass('panel-collapsed');
		$this.find('i').removeClass('glyphicon-chevron-up').addClass('glyphicon-chevron-down');
	} else {
		$this.parents(".result-group").find('.panel-body').slideDown();
		$this.removeClass('panel-collapsed');
		$this.find('i').removeClass('glyphicon-chevron-down').addClass('glyphicon-chevron-up');
	}
});

$('#filterPattern').on('keypress', function(e) {
	if (e.keyCode == 13) {
		e.preventDefault();
		var $panels = $('.result-group');
	    var val = $(this).val().toLowerCase();

	    $panels.show().filter(function() {
	        var panelTitleText = $(this).find('.panel-title').text().toLowerCase();
	        return panelTitleText.indexOf(val) < 0;
	    }).hide();
    }
});