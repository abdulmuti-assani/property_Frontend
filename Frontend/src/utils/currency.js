// src/utils/currency.js

export const CURRENCY_SYMBOL = "$";

export const formatPrice = (value) => {
  const num = Number(value) || 0;
  return `${CURRENCY_SYMBOL}${num.toLocaleString("en-US")}`;
};

export const formatCompactPrice = (value) => {
  const num = Number(value) || 0;
  if (num >= 1000000) {
    return `${CURRENCY_SYMBOL}${(num / 1000000).toFixed(1).replace(/\.0$/, "")}M`;
  }
  if (num >= 1000) {
    return `${CURRENCY_SYMBOL}${(num / 1000).toFixed(0)}K`;
  }
  return `${CURRENCY_SYMBOL}${num}`;
};
