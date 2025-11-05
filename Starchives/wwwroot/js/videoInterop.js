// Minimal global helper to seek a YouTube iframe by videoId using postMessage.
// Requires iframe id="iframe-<videoId>" and enablejsapi=1 + origin in the iframe src.
window.starchivesSeekTo = function (videoId, seconds) {
	try {
		const iframe = document.getElementById(`iframe-${videoId}`);
		if (!iframe || !iframe.contentWindow) return;

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
		console.error("starchivesSeekTo error (postMessage)", e);
		try {
			const url = new URL(iframe.src, document.baseURI);
			url.searchParams.set("start", String(seconds));
			url.searchParams.set("autoplay", "1");
			iframe.src = url.toString();
		} catch (e2) {
			console.error("starchivesSeekTo fallback error", e2);
		}
	}
};

// Sync caption-window max-height to the height of the video container.
// Retries several times while the collapse expands, and updates on window resize.
window._starchivesResizeHandlers ||= {};

window.starchivesSyncCaptionHeight = function (videoId) {
	const vc = document.getElementById(`video-container-${videoId}`);
	const cw = document.getElementById(`caption-window-${videoId}`);
	if (!vc || !cw) return;

	const apply = () => {
		const h = vc.getBoundingClientRect().height;
		if (h > 0) {
			cw.style.maxHeight = `${Math.floor(h)}px`;
			cw.style.overflowY = 'auto';
			return true;
		}
		return false;
	};

	// Try immediately, then retry up to N times while the accordion animates
	let tries = 0, maxTries = 15;
	const tick = () => {
		if (apply()) return;
		if (tries++ < maxTries) {
			setTimeout(tick, 100);
		}
	};
	// First attempt after layout
	requestAnimationFrame(() => {
		if (!apply()) tick();
	});

	// Register a resize handler once per videoId
	if (!window._starchivesResizeHandlers[videoId]) {
		const handler = () => apply();
		window._starchivesResizeHandlers[videoId] = handler;
		window.addEventListener('resize', handler);
	}
};

// Register a global handler so whenever ANY accordion is fully expanded we sync its caption height.
// Uses Bootstrap's shown.bs.collapse event. Works across pages without per-item re-registration.
window._starchivesGlobalCollapseListenerRegistered ||= false;
window.starchivesRegisterCaptionSync = function (videoId) {
	// Ensure the global delegated listener is installed once
	if (!window._starchivesGlobalCollapseListenerRegistered) {
		document.addEventListener('shown.bs.collapse', (evt) => {
			const el = evt.target;
			if (!el || !el.id) return;
			const id = el.id;
			if (id.startsWith('collapse-')) {
				const vId = id.substring('collapse-'.length);
				window.starchivesSyncCaptionHeight(vId);
			}
		});
		window._starchivesGlobalCollapseListenerRegistered = true;
	}

	// Also try to sync immediately (covers already-open or quickly toggled cases)
	window.starchivesSyncCaptionHeight(videoId);
};