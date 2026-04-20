console.log("investment.js v23");
var CRM = CRM || {};
debugger;

CRM.Investment = (function () {

    const Fields = {
        InvestedAmount: "mv_amount",
        InvestmentDate: "mv_date",
        InvestorRiskLevel: "mv_investor",
        HighValueInvestment: "mv_highvalueinvestment"
    };


    const Notifications = {
        HighValue: "high_value_investment",
        FutureDate: "future_date_error"
    };

    function getAttributes(formContext) {
        return {
            amount: formContext.getAttribute(Fields.InvestedAmount),
            date: formContext.getAttribute(Fields.InvestmentDate),
            risk: formContext.getAttribute(Fields.InvestorRiskLevel),
            highValue: formContext.getAttribute(Fields.HighValueInvestment)
        };
    }

    function onLoad(executionContext) {

        // Populate portfolio summary on form open
        setInvestorPortfolioSummary(executionContext);
    }

    function onSave(executionContext) {

        setInvestorPortfolioSummary(executionContext);

        confirmNavigationSave(executionContext);
    }

    function calculateInvestment(executionContext) {
        var formContext = executionContext.getFormContext();
        var attrs = getAttributes(formContext);

        if (!attrs.amount)
            return;

        var amount = attrs.amount.getValue();

        if (amount === null || amount === undefined)
            return;

        handleHighValueInvestment(formContext, attrs, amount);
    }

    function handleHighValueInvestment(formContext, attrs, amount) {

        if (!attrs.highValue)
            return;

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

        var formContext = executionContext.getFormContext();
        var attrs = getAttributes(formContext);

        if (!attrs.date)
            return;

        var date = attrs.date.getValue();

        if (!date)
            return;

        var today = new Date();
        today.setHours(0, 0, 0, 0);

        var investmentDate = new Date(date);
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

    function setInvestorPortfolioSummary(executionContext) {

        var formContext = executionContext.getFormContext();

        //ensureOriginalInvestorInitialized(formContext);

        var investor = CRM.Utils.getValue(formContext, "mv_investor");

        if (!investor) {

            CRM.Utils.setValue(
                formContext,
                "mv_investorportfoliosummary",
                null
            );

            return;
        }

        CRM.Utils.retrieveById(
            "mv_investor",
            investor,
            ["mv_totalinvested_calculated"]
        ).then(function (record) {

            if (!record || record.mv_totalinvested_calculated == null) {

                CRM.Utils.setValue(
                    formContext,
                    "mv_investorportfoliosummary",
                    "No existing investments"
                );

                return;
            }

            var total = record.mv_totalinvested_calculated;

            var summary =
                "Total invested | $" +
                total.toLocaleString();

            CRM.Utils.setValue(
                formContext,
                "mv_investorportfoliosummary",
                summary
            );
        });
    } 

    var InvestmentRibbon = (function () {

        function confirmInvestment(executionContext) {

            var formContext = executionContext;

            var confirmStrings = {
                title: "Confirm Investment",
                text: "Are you sure you want to confirm this investment?"
            };

            var confirmOptions = {
                height: 200,
                width: 450
            };

            Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions)
                .then(function (result) {

                    if (!result.confirmed)
                        return;

                    var CONFIRMED_STATUS = 100000001; // your option value

                    formContext.getAttribute("mv_investmentstatus")
                        .setValue(CONFIRMED_STATUS);

                    formContext.data.save();

                });
        }


    return {
        calculateInvestment: calculateInvestment,
        validateInvestmentDate: validateInvestmentDate,
        setInvestorPortfolioSummary: setInvestorPortfolioSummary,
        confirmInvestment: confirmInvestment,
        onLoad: onLoad,
        onSave: onSave
    };

})();