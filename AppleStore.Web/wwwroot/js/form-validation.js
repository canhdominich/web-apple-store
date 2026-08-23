(function ($) {
  if (!$ || !$.validator) return;

  $.extend($.validator.messages, {
    required: "Vui lòng nhập thông tin này.",
    remote: "Vui lòng kiểm tra lại thông tin.",
    email: "Email không đúng định dạng.",
    url: "Đường dẫn không đúng định dạng.",
    date: "Ngày không đúng định dạng.",
    number: "Vui lòng nhập số hợp lệ.",
    digits: "Vui lòng chỉ nhập chữ số.",
    equalTo: "Giá trị xác nhận không khớp.",
    maxlength: $.validator.format("Vui lòng nhập không quá {0} ký tự."),
    minlength: $.validator.format("Vui lòng nhập ít nhất {0} ký tự."),
    rangelength: $.validator.format("Vui lòng nhập từ {0} đến {1} ký tự."),
    range: $.validator.format("Vui lòng nhập giá trị từ {0} đến {1}."),
    max: $.validator.format("Vui lòng nhập giá trị nhỏ hơn hoặc bằng {0}."),
    min: $.validator.format("Vui lòng nhập giá trị lớn hơn hoặc bằng {0}.")
  });

  function closestField(element) {
    return $(element).closest(".form-field, .mb-3, .mb-4, .col-12, .col-md-5, .col-md-6, .col-md-7");
  }

  function updateSubmitState(form) {
    const $form = $(form);
    const required = $form.find("[data-val-required], [required]").filter(":enabled");
    const hasMissing = required.toArray().some((el) => !String($(el).val() || "").trim());
    const validator = $form.data("validator");
    const hasErrors = validator ? validator.numberOfInvalids() > 0 : false;
    $form.find(".js-submit-button").prop("disabled", hasMissing || hasErrors);
  }

  $.validator.setDefaults({
    onfocusout: function (element) {
      this.element(element);
      updateSubmitState(element.form);
    },
    onkeyup: function (element) {
      if ($(element).data("valRequired") || element.hasAttribute("required")) updateSubmitState(element.form);
    },
    errorElement: "span",
    errorClass: "field-validation-error",
    highlight: function (element) {
      $(element).addClass("is-invalid input-validation-error");
      closestField(element).addClass("field-invalid");
    },
    unhighlight: function (element) {
      $(element).removeClass("is-invalid input-validation-error");
      if (!closestField(element).find(".is-invalid").length) closestField(element).removeClass("field-invalid");
    },
    errorPlacement: function (error, element) {
      error.prepend('<i class="bi bi-exclamation-circle me-1"></i>');
      const target = element.closest(".form-field").find("[data-valmsg-for='" + element.attr("name") + "']");
      if (target.length) target.replaceWith(error.attr("data-valmsg-for", element.attr("name")));
      else error.insertAfter(element);
    }
  });

  $(function () {
    $("form").each(function () {
      if ($.validator.unobtrusive) $.validator.unobtrusive.parse(this);
      updateSubmitState(this);
    });

    $(document).on("input change blur", "form input, form textarea, form select", function () {
      updateSubmitState(this.form);
    });

    $(document).on("click", ".js-use-saved-address", function () {
      const form = this.closest("form");
      const address = $(this).data("address") || "";
      const input = $(form).find("[name='ShippingAddress']");
      input.val(address).trigger("input").trigger("blur");
    });

    $(document).on("submit", ".js-checkout-form", function () {
      const form = this;
      if ($(form).valid && !$(form).valid()) return false;
      const button = $(form).find(".js-submit-button");
      button.prop("disabled", true);
      button.find(".spinner-border").removeClass("d-none");
      button.find(".submit-label").text("Đang xử lý");
    });
  });
})(window.jQuery);
