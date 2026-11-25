import { InputHTMLAttributes } from "react";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  isRequired?: boolean;
}

export default function Input({ label, id, isRequired, className = "", ...props }: InputProps) {
  return (
    <div>
      <label htmlFor={id} className="mb-1 block text-sm font-medium text-gray-700">
        {label}
        {isRequired && <span className="text-red-600"> *</span>}
      </label>
      <input
        id={id}
        className={`w-full rounded border border-gray-300 bg-white px-3 py-2 text-sm focus:border-blue-500 focus:outline-none ${className}`}
        {...props}
      />
    </div>
  );
}