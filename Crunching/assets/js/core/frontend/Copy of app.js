function changeStatus() {
	var getText = $('.change-status .dropdown-menu li button');
	var button = $('.change-status .btn:first-child');
	$(getText).on( 'click', button, function() {
		var text = $(this).html();
		$(this).parents(".change-status").find(".btn:first-child").html(text + ' <i class="fa fa-caret-down"></i>').val(text);
	});
};
changeStatus();

function format(value) {
	return '<div>' + value + '</div>';
}

function dataTable() {
	$(document).ready( function () {
        var table = $('.dataTable-accordion').DataTable({
			destroy: true,
			paging: false,
			bFilter: false,
			"order": [],
			"columnDefs": [ {
				"targets"  : 'no-sort',
				"orderable": false,
			}],
            "language": {
              "emptyTable": "Well done, no open tasks"
            }
		});
		$('.dataTable-accordion tbody').on('click', '.open-button', function () {
			var tr = $(this).closest('tr');
			var row = table.row(tr);

			if (row.child.isShown()) {
				row.child.hide();
				tr.removeClass('shown');
			} else {
				row.child(format(tr.data('child-value'))).show();
				tr.addClass('shown');
			}
		});
	});
	/*$(document).ready( function () {
		$('.dataTable').DataTable({
			destroy: true,
			paging: false,
			bFilter: false,
			"order": [],
			"columnDefs": [ {
				"targets"  : 'no-sort',
				"orderable": false,
			}],
			"language": {
				"emptyTable": "No visit reports available"
			}
		});
	});*/

}
dataTable();

function datepicker() {

	$( "#datepicker" ).datepicker({
		todayHighlight: true,
		autoclose: true,
		changeMonth: true,
		changeYear: true,
		dateFormat: 'mm/dd/yy',
		onSelect: function(){
			$('#summernoteForm').bootstrapValidator('revalidateField', 'deadline');
		}
	});

}
datepicker();

function summernote() {
	if ( $( "#summernote" )[0] ) {

		function validateEditor() {
			$('#summernoteForm').bootstrapValidator('revalidateField', 'content');
		};

		$('#summernoteForm').bootstrapValidator({
			excluded: [':disabled'],
			feedbackIcons: {
				valid: 'glyphicon glyphicon-ok',
				invalid: 'glyphicon glyphicon-remove',
				validating: 'glyphicon glyphicon-refresh'
			},
			fields: {
				visitDate: {
					validators: {
						notEmpty: {
							message: 'The date is required and cannot be empty'
						},
						date: {
							format: 'MM/DD/YYYY',
							message: 'The date is not a valid'
						}
					}
				},
				deadline: {
					validators: {
						notEmpty: {
							message: 'The date is required and cannot be empty'
						},
						date: {
							format: 'MM/DD/YYYY',
							message: 'The date is not a valid'
						}
					}
				},
				selectResponsible: {
					validators: {
						notEmpty: {
							message: 'Please choose responsbile'
						}
					}
				},
				selectClient: {
					validators: {
						notEmpty: {
							message: 'Please choose client'
						}
					}
				},
				content: {
					validators: {
						callback: {
							message: 'The content is required and cannot be empty',
							callback: function(value, validator, $field) {
								var code = $('[name="content"]').summernote('code');
								return (code !== '' && code !== '<p><br></p>');
							}
						}
					}
				}
			}
		})
		.on('error.field.bv', '[name="content"]', function(e, data) {
			var containerError = "<div id='errorHead' class='error-Head'>An error occured. Please check the highlighted fields below. <button id='removeError'><i class='md md-close'></i></button></div>";
			$('.backlink').after(containerError);
			$('#removeError').click(function () {
				$('#errorHead').remove();
			});
		})
		.on('changeDate', function(e) {
			$('#summernoteForm').bootstrapValidator('revalidateField', 'visitDate');
            $('#summernoteForm').bootstrapValidator('revalidateField', 'deadline');
		})
        .on('summernote.change', function(customEvent, contents, $editable) {
			$('#summernoteForm').bootstrapValidator('revalidateField', 'content');
		})
		.find('[name="content"]').each(function () {
		    $(this).addClass('summernote');
	    }).summernote({
			height: $('#summernote').height(),
			toolbar: [
				['style', ['bold', 'italic', 'underline', 'clear']],
				['fontsize', ['fontsize']],
				['color', ['color']],
				['para', ['ul', 'ol', 'paragraph']],
				['float', ['floatLeft', 'floatRight', 'floatNone']],
				['insert', ['link', 'picture', 'hr']],
				['remove', ['removeMedia']]
			],
			onkeyup: function() {
				validateEditor();
			},
			onpaste: function() {
				validateEditor();
			},
            callbacks: {
				onEnter: function(e){
					var summerOE = $('.summernote');
					summerOE.summernote('pasteHTML','');
					e.preventDefault();
				}
			}
		});
	}

}
summernote();

function summernoteSmall() {
	if ( $( "#summernoteSmall" )[0] ) {

		function validateEditor() {
			$('#summernoteForm').bootstrapValidator('revalidateField', 'content');
		};

		$('#summernoteForm').bootstrapValidator({
			excluded: [':disabled'],
			feedbackIcons: {
				valid: 'glyphicon glyphicon-ok',
				invalid: 'glyphicon glyphicon-remove',
				validating: 'glyphicon glyphicon-refresh'
			},
			fields: {
				visitDate: {
					validators: {
						notEmpty: {
							message: 'The date is required and cannot be empty'
						},
						date: {
							format: 'MM/DD/YYYY',
							message: 'The date is not a valid'
						}
					}
				},
				deadline: {
					validators: {
						notEmpty: {
							message: 'The date is required and cannot be empty'
						},
						date: {
							format: 'MM/DD/YYYY',
							message: 'The date is not a valid'
						}
					}
				},
				selectResponsible: {
					validators: {
						notEmpty: {
							message: 'Please choose responsbile'
						}
					}
				},
				selectClient: {
					validators: {
						notEmpty: {
							message: 'Please choose client'
						}
					}
				},
				content: {
					validators: {
						callback: {
							message: 'The content is required and cannot be empty',
							callback: function(value, validator, $field) {
								var code = $('[name="content"]').summernote('code');
								return (code !== '' && code !== '<p><br></p>');
							}
						}
					}
				}
			}
		})
		.on('error.field.bv', '[name="content"]', function(e, data) {
			var containerError = "<div id='errorHead' class='error-Head'>An error occured. Please check the highlighted fields below. <button id='removeError'><i class='md md-close'></i></button></div>";
			$('.backlink').after(containerError);
			$('#removeError').click(function () {
				$('#errorHead').remove();
			});
		})
		.on('changeDate', function(e) {
			$('#summernoteForm').bootstrapValidator('revalidateField', 'visitDate');
            $('#summernoteForm').bootstrapValidator('revalidateField', 'deadline');
		})
        .on('summernote.change', function(customEvent, contents, $editable) {
			$('#summernoteForm').bootstrapValidator('revalidateField', 'content');
		})
		.find('[name="content"]').each(function () {
		    $(this).addClass('summernote');
	    }).summernote({
			height: $('#summernoteSmall').height(),
			toolbar: [
				['style', ['bold', 'italic', 'underline', 'clear']],
				['fontsize', ['fontsize']],
				['color', ['color']],
			],
			onkeyup: function() {
				validateEditor();
			},
			onpaste: function() {
				validateEditor();
			},
            callbacks: {
				onEnter: function(e){
					var summerOE = $('.summernote');
					summerOE.summernote('pasteHTML','');
					e.preventDefault();
				}
			}
		});
	}

}
summernoteSmall();

function lightbox() {
	$(document).on('click', '[data-toggle="lightbox"]', function(event) {
		event.preventDefault();
		$(this).ekkoLightbox();
	});
}
lightbox();

function smoothScroll() {
	$("a.addtask").click(function() {
		$('html, body').animate({
			scrollTop: $("#addtask").offset().top -140
		}, 1000);
	});
}
smoothScroll();

function stuff() {
	$("#sendform").click(function() {
		$("#summernoteForm").submit();
	});

	var fadeStart=0 // 100px scroll or less will equiv to 1 opacity
	,fadeUntil=200 // 200px scroll or more will equiv to 0 opacity
	,fading = $('.steps:before')
	;

	$(window).bind('scroll', function(){
	var offset = $(document).scrollTop()
		,opacity=0
	;
	if( offset<=fadeStart ){
		opacity=1;
	}else if( offset<=fadeUntil ){
		opacity=1-offset/fadeUntil;
	}
	fading.css('opacity',opacity);
	});

}
stuff();
