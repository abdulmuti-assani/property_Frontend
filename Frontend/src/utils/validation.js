// Shared client-side validation. The backend enforces the same rules — these
// just give the user immediate feedback.

// A person's name must contain letters — pure numbers are rejected.
export const isValidName = (value) => {
  const trimmed = (value || "").trim();
  return trimmed.length >= 2 && (trimmed.match(/\p{L}/gu) || []).length >= 2;
};

// Phone is optional; when given it must be 7–15 digits, with only
// +, spaces, dashes or parentheses as separators (no letters).
export const isValidPhone = (value) => {
  const trimmed = (value || "").trim();
  if (!trimmed) return true;
  if (!/^[0-9+()\-\s]+$/.test(trimmed)) return false;
  const digits = (trimmed.match(/\d/g) || []).length;
  return digits >= 7 && digits <= 15;
};

// Strip anything that isn't allowed in a phone field, as the user types.
export const sanitizePhoneInput = (value) =>
  (value || "").replace(/[^0-9+()\-\s]/g, "");
