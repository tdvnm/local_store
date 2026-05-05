/**
 * SignalR real-time connection for order updates.
 * 
 * This module does NOT import @microsoft/signalr directly — the caller
 * passes the HubConnectionBuilder so the dependency stays in the app.
 * 
 * Usage (from app):
 *   import { HubConnectionBuilder, LogLevel, HttpTransportType } from '@microsoft/signalr';
 *   import { createOrderHub } from '@society-commerce/api-client/realtime';
 *   const hub = createOrderHub('http://localhost:5000/api', () => token, {
 *     HubConnectionBuilder, LogLevel, HttpTransportType,
 *   });
 *   hub.onNewOrder((data) => { ... });
 *   hub.onOrderUpdated((data) => { ... });
 *   await hub.start();
 */

export interface OrderHubClient {
	onNewOrder: (handler: (data: { id: string; orderNumber: string }) => void) => void;
	onOrderUpdated: (handler: (data: { id: string; status?: number; confirmedTotalPaise?: number }) => void) => void;
	start: () => Promise<void>;
	stop: () => Promise<void>;
	readonly connected: boolean;
}

/** Minimal subset of @microsoft/signalr types we need */
export interface SignalRDeps {
	HubConnectionBuilder: new () => any;
	LogLevel: { Warning: any };
	HttpTransportType: { WebSockets: any; ServerSentEvents: any; LongPolling: any };
}

export function createOrderHub(
	baseUrl: string,
	getToken: () => string | null,
	signalr: SignalRDeps,
): OrderHubClient {
	const hubUrl = baseUrl.replace(/\/api\/?$/, '') + '/hubs/orders';

	const connection = new signalr.HubConnectionBuilder()
		.withUrl(hubUrl, {
			accessTokenFactory: () => getToken() ?? '',
			transport: signalr.HttpTransportType.WebSockets | signalr.HttpTransportType.ServerSentEvents | signalr.HttpTransportType.LongPolling,
		})
		.withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
		.configureLogging(signalr.LogLevel.Warning)
		.build();

	let _connected = false;

	connection.onclose(() => { _connected = false; });
	connection.onreconnecting(() => { _connected = false; });
	connection.onreconnected(() => { _connected = true; });

	return {
		get connected() { return _connected; },

		onNewOrder(handler) {
			connection.on('NewOrder', handler);
		},

		onOrderUpdated(handler) {
			connection.on('OrderUpdated', handler);
		},

		async start() {
			try {
				await connection.start();
				_connected = true;
				console.log('[SignalR] Connected to order hub');
			} catch (err) {
				console.warn('[SignalR] Failed to connect:', err);
				_connected = false;
			}
		},

		async stop() {
			try {
				await connection.stop();
			} catch { /* ignore */ }
			_connected = false;
		},
	};
}
