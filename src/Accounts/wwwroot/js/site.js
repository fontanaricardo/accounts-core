$(document).ready(function () {

	noCopyPaste();
	maskPhones();
	maskZipCodes();
	maskCpf();
	disableAutoComplete();
	togglePassword();
	togglePasswordCapsLock();
	updateSignature();

});

function maskPhones() {
	$('.phone').mask("(99) 99999999?9");
}

function maskZipCodes() {
	$('.zipcode').mask("99999999");
}

function maskCpf() {
	$(".cpf").mask("99999999999");
}

function noCopyPaste() {
	$(".no-copy-paste").bind("cut copy paste", function (e) {
		e.preventDefault();
	});
}

function disableAutoComplete() {
	$("form").attr("autocomplete", "off");
	$("input").attr("autocomplete", "off");
}

function toggleMessage(summaryId, message, condition) {
	liWithMessage = '<li>"' + message + '"</li>';

	if (condition) {
		$("#" + summaryId + " ul").append(liWithMessage);
	}
	else {
		$("li:contains(" + message + ")").remove();
	}
}

function togglePassword() {
	$(".password-button").mousedown(function () {
		$(this).parent().parent().find('input[type=password]').prop("type", "text");
	}).bind('mouseup mouseleave', function () {
		$(this).parent().parent().find('input[type=text]').prop("type", "password");
	});

	$(".password-button").focus(function () {
		$(this).parent().next('input').focus();
	});

}

function togglePasswordCapsLock() {
	$(".password-input").keypress(function (e) {

		kc = e.keyCode ? e.keyCode : e.which;
		sk = e.shiftKey ? e.shiftKey : ((kc == 16) ? true : false);
		if (((kc >= 65 && kc <= 90) && !sk) || ((kc >= 97 && kc <= 122) && sk)) {
			$(this).parent().parent().find('.caps-lock-warning').show();
		}
		else {
			$(this).parent().parent().find('.caps-lock-warning').hide();
		}
	});

	$(".password-input").blur(function () {
		$(this).parent().parent().find('.caps-lock-warning').hide();
	});
}

function updateSignature() {
	$.ajax({
		url: "/Certification/Status",
	})
	.done(function (data) {
		if (data == 0 || data == 1) {
			$("#certStatus").html("Não");
			$("#certStatus").addClass("text-danger");
		} else {
			$("#certStatus").html("Sim");
			$("#certStatus").addClass("text-success");
		}

		showCertLinks(data);
	});
}

function showCertLinks(status) {
	if ($("#certLink").length && $("#addDocLink").length) {

		if (status == 0) {
			$("#certLink").show();
			$("#addDocLink").hide();
		} else if (status == 1) {
			$("#certLink").hide();
			$("#addDocLink").show();
		}

	}
}

function buscarCep() {
	cep = $("input[name*='ZipCode']").val().trim();

	$.getJSON("https://virtual.joinville.sc.gov.br/Endereco/Logradouros/Index?cep=" + cep, function (data) {

		var firstRow = data.registros[0];

		if (firstRow.Tipo === undefined || firstRow.Logradouro === undefined) {
			$("input[name*='Street']").prop("readonly", false);
			$("input[name*='Street']").val("");
		} else {
			$("input[name*='Street']").val(firstRow.Tipo + " " + firstRow.Logradouro);
			$("input[name*='Street']").prop("readonly", true);
		}

		if (firstRow.Bairro === undefined) {
			$("input[name*='District']").prop("readonly", false);
			$("input[name*='District']").val("");
		} else {
			$("input[name*='District']").val(firstRow.Bairro);
			$("input[name*='District']").prop("readonly", true);
		}

		if (firstRow.Cidade === undefined) {
			$("input[name*='City']").prop("readonly", false);
			$("input[name*='City']").val("");
		} else {
			$("input[name*='City']").val(firstRow.Cidade);
			$("input[name*='City']").prop("readonly", true);
		}

		if (firstRow.UF === undefined) {
			$("input[name*='State']").prop("readonly", false);
			$("input[name*='State']").val("");
		} else {
			$("input[name*='State']").val(firstRow.UF);
			$("input[name*='State']").prop("readonly", true);
		}

	}).fail(function () {
		$("input[name*='Street']").prop("readonly", false);
		$("input[name*='Street']").val("");
		$("input[name*='District']").prop("readonly", false);
		$("input[name*='District']").val("");
		$("input[name*='City']").prop("readonly", false);
		$("input[name*='City']").val("");
		$("input[name*='State']").prop("readonly", false);
		$("input[name*='State']").val("");
		alert("CEP não encontrado");
	}).always(function () {
		$("#btnBuscarCEP").prop("disabled", false);
	});
}
