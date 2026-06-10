var CRM = CRM || {};

CRM.Investor = (function () {

    const Notifications = {
        InvalidEmail: "invalid_email"
    };

    const Schema = {

        Fields: {
            Risk: "mv_risklevel"
        },

        RiskLevel: {
            Low: 124730000,
            Medium: 124730001,
            High: 124730002
        }
    };

    function getAttributes(formContext) {

        return {
            risk: formContext.getAttribute(
                Schema.Fields.Risk
            )
        };
    }

    function onLoad(executionContext) {

        updateRiskLevel(executionContext);
    }

    function onSave(executionContext) {
    }

    function validateEmail(executionContext) {

        var formContext =
            executionContext.getFormContext();

        var emailAttr =
            formContext.getAttribute("mv_email");

        if (!emailAttr) return;

        var email = emailAttr.getValue();

        if (!email) {

            formContext.ui.clearFormNotification(
                Notifications.InvalidEmail
            );

            return;
        }

        email = email.trim();

        var atCount =
            (email.match(/@/g) || []).length;

        var isValid =
            atCount === 1 &&
            !email.includes(" ");

        if (!isValid) {

            formContext.ui.setFormNotification(
                "Please enter a valid email address.",
                "ERROR",
                Notifications.InvalidEmail
            );

        } else {

            formContext.ui.clearFormNotification(
                Notifications.InvalidEmail
            );
        }
    }

    function updateRiskLevel(executionContext) {

        var formContext =
            executionContext.getFormContext();

        var attrs =
            getAttributes(formContext);

        var total =
            formContext.getAttribute(
                "mv_totalinvested"
            )?.getValue();

        if (total == null)
            return;

        var riskLevel = null;

        if (total >= 100000) {

            riskLevel =
                Schema.RiskLevel.High;

        } else if (total >= 10000) {

            riskLevel =
                Schema.RiskLevel.Medium;

        } else {

            riskLevel =
                Schema.RiskLevel.Low;
        }

        attrs.risk.setValue(riskLevel);
    }

    return {

        validateEmail: validateEmail,
        updateRiskLevel: updateRiskLevel,
        onLoad: onLoad,
        onSave: onSave
    };

})();