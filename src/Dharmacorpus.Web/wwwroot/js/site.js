/* Dharmacorpus site behaviour (jQuery). Everything here is progressive enhancement:
   all content and the enquiry form work without JavaScript. */
(function ($) {
  "use strict";

  document.documentElement.classList.add("js");

  /* ---- Disclaimer gate (BCI Rule 36), remembered for 30 days ---- */
  var GATE_KEY = "dc.disclaimerAcceptedAt";
  var GATE_DAYS = 30;

  function gateAccepted() {
    try {
      var at = parseInt(window.localStorage.getItem(GATE_KEY) || "0", 10);
      return at && Date.now() - at < GATE_DAYS * 864e5;
    } catch (e) {
      return false;
    }
  }

  function initGate() {
    var $gate = $("#disclaimerGate");
    if (!$gate.length || gateAccepted() || window.location.pathname === "/disclaimer") return;
    var lastFocus = document.activeElement;
    $gate.removeClass("hidden").addClass("flex");
    $("body").addClass("overflow-hidden");
    $gate.find("[data-gate-accept]").trigger("focus");

    // Keep keyboard focus inside the dialog.
    $gate.on("keydown", function (e) {
      if (e.key !== "Tab") return;
      var $f = $gate.find("a[href], button").filter(":visible");
      var first = $f.get(0), last = $f.get($f.length - 1);
      if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
      else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
    });

    $gate.find("[data-gate-accept]").on("click", function () {
      try { window.localStorage.setItem(GATE_KEY, String(Date.now())); } catch (e) { /* private mode */ }
      $gate.addClass("hidden").removeClass("flex");
      $("body").removeClass("overflow-hidden");
      if (lastFocus && lastFocus.focus) lastFocus.focus();
    });
  }

  /* ---- Practice area filter chips ---- */
  function initPracticeFilter() {
    $("[data-practice-filter]").each(function () {
      var $bar = $(this);
      var $grid = $bar.nextAll("[data-practice-grid]").first();
      $bar.on("click", "[data-filter]", function () {
        var f = $(this).data("filter");
        $bar.find("[data-filter]").attr("aria-selected", "false");
        $(this).attr("aria-selected", "true");
        $grid.children("[data-group]").each(function () {
          var show = f === "all" || $(this).data("group") === f;
          $(this).toggleClass("hidden", !show);
          if (show) $(this).addClass("is-visible");
        });
      });
    });
  }

  /* ---- Scroll reveal (respects reduced motion via CSS) ---- */
  function initReveal() {
    var $items = $(".reveal");
    if (!("IntersectionObserver" in window)) { $items.addClass("is-visible"); return; }
    var io = new IntersectionObserver(function (entries) {
      entries.forEach(function (en) {
        if (en.isIntersecting) { en.target.classList.add("is-visible"); io.unobserve(en.target); }
      });
    }, { rootMargin: "0px 0px -40px 0px" });
    $items.each(function () { io.observe(this); });
  }

  /* ---- Enquiry form: character counter + AJAX submit ---- */
  function initEnquiryForm() {
    var $form = $("#enquiryForm");
    if (!$form.length) return;

    $form.find("[data-counter]").each(function () {
      var $t = $(this), $c = $($t.data("counter"));
      var update = function () { $c.text($t.val().length + " / 600"); };
      $t.on("input", update); update();
    });

    $form.on("submit", function (e) {
      e.preventDefault();
      var $btn = $form.find("[type=submit]");
      var $err = $("#enquiryError");
      $form.find(".field-error").text("");
      $form.find(".input-validation-error").removeClass("input-validation-error").removeAttr("aria-invalid");
      $err.addClass("hidden").text("");
      $btn.prop("disabled", true).text("Sending…");

      $.ajax({
        url: $form.attr("action"),
        method: "POST",
        data: $form.serialize(),
        headers: { "X-Requested-With": "XMLHttpRequest" }
      }).done(function () {
        $form.addClass("hidden");
        $("#enquirySuccess").removeClass("hidden").trigger("focus");
      }).fail(function (xhr) {
        var res = xhr.responseJSON || {};
        if (xhr.status === 400 && res.errors) {
          var firstBad = null;
          $.each(res.errors, function (key, msg) {
            $form.find('[data-valmsg-for="' + key + '"]').text(msg);
            var $input = $form.find('[name="' + key + '"]');
            $input.addClass("input-validation-error").attr("aria-invalid", "true");
            if (!firstBad) firstBad = $input.get(0);
          });
          if (firstBad) firstBad.focus();
        } else if (xhr.status === 429) {
          $err.text("Too many enquiries from this connection. Please call or email the office directly.").removeClass("hidden");
        } else {
          $err.text(res.message || "Your enquiry could not be sent. Please call or email the office directly.").removeClass("hidden");
        }
      }).always(function () {
        $btn.prop("disabled", false).text($btn.data("submit-label"));
      });
    });
  }

  $(function () {
    initGate();
    initPracticeFilter();
    initReveal();
    initEnquiryForm();
  });
})(jQuery);
