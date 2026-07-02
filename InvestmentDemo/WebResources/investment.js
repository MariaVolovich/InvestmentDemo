var CRM = CRM || {};

CRM.Investment = (function () {
    const Schema = {
        Fields: {
            Amount: "mv_amount",
            Date: "mv_date",
            ExitDate: "mv_exitdate",
            Investor: "mv_investor",
            Opportunity: "mv_investmentopportunity",
            Lifecycle: "mv_lifecycle",
            HighValue: "mv_highvalueinvestment",
            PortfolioSummary: "mv_investorportfoliosummary"
        },
        InvestmentStatus: {
            Confirmed: 124730000,
            Draft: 124730001
        }
        
    };

    const Notifications = {
        HighValue: "high_value_investment",
        FutureDate: "future_date_error",
        ExitDateValidation: "exit_date_validation"
    };


    function getAttributes(formContext) {
        return {
            amount: formContext.getAttribute(Schema.Fields.Amount),
            date: formContext.getAttribute(Schema.Fields.Date),
            exitDate: formContext.getAttribute(Schema.Fields.ExitDate),
            investor: formContext.getAttribute(Schema.Fields.Investor),
            highValue: formContext.getAttribute(Schema.Fields.HighValue),
            lifecycle: formContext.getAttribute(Schema.Fields.Lifecycle)
        };
    }


    function onLoad(executionContext) {

        setInvestorPortfolioSummary(executionContext);

        lockFieldsIfConfirmed(executionContext);

        setDefaultInvestmentDate(executionContext);
    }

    function onSave(executionContext) {

        setInvestorPortfolioSummary(executionContext);
    }

    function calculateInvestment(executionContext) {

        var formContext = executionContext.getFormContext();
        var attrs = getAttributes(formContext);

        if (!attrs.amount) return;

        var amount = attrs.amount.getValue();

        if (amount == null) return;

        handleHighValueInvestment(formContext, attrs, amount);
    }

    function handleHighValueInvestment(formContext, attrs, amount) {

        if (!attrs.highValue) return;

        var isHighValue = amount >= 50000;

        attrs.highValue.setValue(isHighValue);

        if (isHighValue) {

            CRM.Utils.showNotification(
                formContext,
                "High-value investment detected.",
                "WARNING",
                Notifications.HighValue
            );

        } else {

            CRM.Utils.clearNotification(
                formContext,
                Notifications.HighValue
            );
        }
    }

    function validateInvestmentDate(executionContext) {

        var formContext =
            executionContext.getFormContext();

        var attrs =
            getAttributes(formContext);

        if (!attrs.date)
            return;

        var date =
            attrs.date.getValue();

        if (!date)
            return;

        var today = new Date();

        today.setHours(0, 0, 0, 0);

        var investmentDate =
            new Date(date);

        investmentDate.setHours(0, 0, 0, 0);

        if (investmentDate > today) {

            CRM.Utils.showNotification(
                formContext,
                "Investment Date cannot be in the future.",
                "ERROR",
                Notifications.FutureDate
            );

            attrs.date.setValue(null);

        } else {

            CRM.Utils.clearNotification(
                formContext,
                Notifications.FutureDate
            );
        }
    }

    function validateExitDate(executionContext) {

        var formContext =
            executionContext.getFormContext();

        var attrs =
            getAttributes(formContext);

        if (!attrs.date || !attrs.exitDate)
            return;

        var investmentDate =
            attrs.date.getValue();

        var exitDate =
            attrs.exitDate.getValue();

        var exitDateControl =
            formContext.getControl(
                Schema.Fields.ExitDate
            );

        if (!investmentDate || !exitDate) {

            if (exitDateControl) {
                CRM.Utils.clearNotification(
                    formContext,
                    Notifications.ExitDateValidation
                );  
            }

            return;
        }

        if (exitDate < investmentDate) {

            CRM.Utils.showNotification(
                formContext,
                "Exit Date cannot be earlier than Investment Date.",
                "ERROR",
                Notifications.ExitDateValidation
            );

            attrs.exitDate.setValue(null);

        } else {
           

            CRM.Utils.clearNotification(
                formContext,
                Notifications.ExitDateValidation
                );                       
        }
    }

    function setDefaultInvestmentDate(executionContext) {

        var formContext = executionContext.getFormContext();

        var formType = formContext.ui.getFormType();

        if (formType !== 1) return;

        var attrs = getAttributes(formContext);

        if (!attrs.date) return;

        if (attrs.date.getValue()) return;

        attrs.date.setValue(new Date());
    }

    // Display portfolio summary from server-side confirmed totals.
    function setInvestorPortfolioSummary(executionContext) {

        var formContext = executionContext.getFormContext();

        var investor = CRM.Utils.getValue(
            formContext,
            Schema.Fields.Investor
        );

        if (!investor) {
            CRM.Utils.setValue(
                formContext,
                Schema.Fields.PortfolioSummary,
                null
            );
            return;
        }

        CRM.Utils.retrieveById(
            "mv_investor",
            investor,
            ["mv_totalinvested", "mv_fullname"]
        ).then(function (record) {

            if (!record || record.mv_totalinvested === null || record.mv_totalinvested === undefined) {
                CRM.Utils.setValue(
                    formContext,
                    Schema.Fields.PortfolioSummary,
                    "No confirmed investments"
                );

                return;
            }

            var total = record.mv_totalinvested;
            var fullName = record.mv_fullname;

            var summary =
                fullName +
                " has invested $" +
                total.toLocaleString() +
                " across confirmed investments";

            CRM.Utils.setValue(
                formContext,
                Schema.Fields.PortfolioSummary,
                summary
            );
        });
    }

    // Confirmed investments are locked on the form; server-side plugins enforce the same rule.
    function lockFieldsIfConfirmed(executionContext) {
        var formContext = executionContext.getFormContext();
        var attrs = getAttributes(formContext);

        if (!attrs.lifecycle) return;

        var lifecycle = attrs.lifecycle.getValue();

        var isConfirmed =
            lifecycle === Schema.InvestmentStatus.Confirmed;

        var fieldsToLock = [
            Schema.Fields.Investor,
            Schema.Fields.Amount,
            Schema.Fields.Opportunity,
            Schema.Fields.Date
        ];

        fieldsToLock.forEach(function (field) {

            var ctrl = formContext.getControl(field);

            if (ctrl) {
                ctrl.setDisabled(isConfirmed);
            }
        });
    }   

    return {
        calculateInvestment: calculateInvestment,
        validateInvestmentDate: validateInvestmentDate,
        setInvestorPortfolioSummary: setInvestorPortfolioSummary,
        setDefaultInvestmentDate: setDefaultInvestmentDate,
        lockFieldsIfConfirmed: lockFieldsIfConfirmed,
        validateExitDate: validateExitDate,
        onLoad: onLoad,
        onSave: onSave
    };

})();