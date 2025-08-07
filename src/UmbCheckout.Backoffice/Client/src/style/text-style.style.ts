import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css } from '@umbraco-cms/backoffice/external/lit';

export const UmbCheckoutTextStyles = css`
	${UUITextStyles}

    .red {
        color: #d42054;
    }

    .bold {
        font-weight: 700;
    }

    .underline {
        text-decoration: underline;
    }
`;