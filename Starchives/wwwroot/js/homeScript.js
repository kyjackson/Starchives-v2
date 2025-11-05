/*
 * This script contains everything necessary for the search functionality on the home page to operate as intended.
 */

function initializeHomePage() {

	// first set up Youtube Iframe API
	let tag = document.createElement('script');
	tag.src = "https://www.youtube.com/iframe_api";
	let firstScriptTag = document.getElementsByTagName('script')[0];
	firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);

	// wait for Iframe API to be ready before searching
	function onYouTubeIframeAPIReady() {
		console.log("Iframe ready.");
		return;
	}

	$("#loadingIcon").hide();
	$(".filters").hide();
	$("#resultHeader").hide();

	// ensure default order is descending every time the page is loaded
	$("#order").val("DESC");
	$("#order").prop("checked", false);

	/**
	 * Enable/disable filters if "Filter" switch is toggled.
	 */
	$("#filterSwitch").prop("checked", false);
	$("#filterSwitch").on("change", function (event) {
		if (!$(".filters").is(":visible")) {
			$(".filters").toggle(true);
		} else {
			$(".filters").toggle(false);
			$("#publishDate").val("");
			$("#duration").val("");
			$("#orderBy").val("1");
			$("#order").prop("checked", false);
			$("#order").val("DESC");
		}
	});

	/**
	 * Change value of "Descending" based on whether the checkbox is checked.
	 */
	$("#order").on("change", function (event) {
		if ($("#order").val() == "DESC") {
			$("#order").val("ASC");
		} else {
			$("#order").val("DESC");
		}
	});

	let resultsLengthFound = false;
	let totalPages = 0;
	let dataToSend = {};
	$("form").on("submit", function (event) {
		event.preventDefault();

		let keyword = $("#searchBar").val();
		let videoPublishDate = $("#publishDate").val();
		let videoDuration = $("#duration").val();
		let orderResultsBy = $("#orderBy").val();
		let orderResults = $("#order").val();
		let pageNumber = 0;

		dataToSend = {
			query: keyword,
			date: videoPublishDate,
			duration: videoDuration,
			orderBy: orderResultsBy,
			order: orderResults,
			page: pageNumber
		};

		resultsLengthFound = false;
		totalPages = 0;
		// Blazor now handles search via @onsubmit. Old AJAX calls left here for reference.
	});

	/* ... existing helper functions (getResults, getResultsLength, showPage, etc.) remain unchanged ... */
}

/**
 * Global helper to seek a YouTube iframe by videoId using postMessage.
 * Requires the iframe to have id="iframe-<videoId>" and enablejsapi=1 in its src.
 */
window.starchivesSeekTo = function (videoId, seconds) {
	try {
		const iframe = document.getElementById(`iframe-${videoId}`);
		if (!iframe || !iframe.contentWindow) return;

		// Seek and then play
		iframe.contentWindow.postMessage(JSON.stringify({
			event: "command",
			func: "seekTo",
			args: [seconds, true]
		}), "*");

		iframe.contentWindow.postMessage(JSON.stringify({
			event: "command",
			func: "playVideo",
			args: []
		}), "*");
	} catch (e) {
		console.error("starchivesSeekTo error", e);
	}
};