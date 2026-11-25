export interface AlertMessage {
  type: 'success' | 'error';
  text: string;
}

const styles: Record<AlertMessage['type'], string> = {
  success: 'bg-green-100 text-green-800',
  error: 'bg-red-100 text-red-800',
};

export default function Alert({ message, onClose }: { message: AlertMessage; onClose?: () => void }) {
  return (
    <div className={`mb-4 flex items-center justify-between rounded p-3 text-sm ${styles[message.type]}`}>
      <span>{message.text}</span>
      {onClose && (
        <button type="button" onClick={onClose} className="ml-4 font-bold">
          ×
        </button>
      )}
    </div>
  );
}
