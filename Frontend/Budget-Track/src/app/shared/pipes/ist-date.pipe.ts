import { Pipe, PipeTransform } from '@angular/core';

const IST: Intl.DateTimeFormatOptions = { timeZone: 'Asia/Kolkata' };

@Pipe({ name: 'istDate', standalone: true, pure: true })
export class IstDatePipe implements PipeTransform {
    transform(value: string | Date | null | undefined, format: 'date' | 'time' | 'datetime' = 'date'): string {
        if (!value) return '—';
        // Backend sends UTC strings without 'Z' (e.g. "2026-03-04T08:30:00").
        // JS parses those as local time, not UTC. Append 'Z' to force UTC parsing.
        const raw = typeof value === 'string'
            ? (value.endsWith('Z') || value.includes('+') ? value : value + 'Z')
            : value;
        const d = typeof raw === 'string' ? new Date(raw) : raw;
        if (isNaN(d.getTime())) return '—';

        if (format === 'date') {
            return d.toLocaleDateString('en-GB', { ...IST, day: '2-digit', month: 'short', year: 'numeric' });
        }
        if (format === 'time') {
            return d.toLocaleTimeString('en-IN', { ...IST, hour: '2-digit', minute: '2-digit', hour12: true });
        }
        // datetime
        const datePart = d.toLocaleDateString('en-GB', { ...IST, day: '2-digit', month: 'short', year: 'numeric' });
        const timePart = d.toLocaleTimeString('en-IN', { ...IST, hour: '2-digit', minute: '2-digit', hour12: true });
        return `${datePart}, ${timePart}`;
    }
}

/** Standalone helper — use in component class methods */
export function toIST(value: string | Date | null | undefined, format: 'date' | 'time' | 'datetime' = 'date'): string {
    const pipe = new IstDatePipe();
    return pipe.transform(value, format);
}
